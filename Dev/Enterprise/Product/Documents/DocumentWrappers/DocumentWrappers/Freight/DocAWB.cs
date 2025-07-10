using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.FormatTables;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocAWB : DocBaseWrapperWithJobHeader, Integration.DocumentWrappers.IDocAWB, IDocTypeCode
	{
		#region Construction

		protected DocAWB(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap)
			: base(exportAWBHeader, factoryToWrap)
		{
			this.IsExportAWBHeaderDeletedAtFirst = exportAWBHeader != null ? exportAWBHeader.IsDeleted : null;
		}

		public bool? IsExportAWBHeaderDeletedAtFirst;

		public static DocAWB New(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap)
		{
			DocAWB result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(exportAWBHeader, factoryToWrap);
			}
			else if (exportAWBHeader != null)
			{
				result = new DocAWB(exportAWBHeader, factoryToWrap);
			}

			if (result != null)
			{
				result.IsInitialized = true;
			}

			return result;
		}

		protected delegate DocAWB NewDelegate(ExportAWBHeader aWB, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Related Business Objects

		protected internal ExportAWBHeader ExportAWBHeader
		{
			get
			{
				var awbHeader = (ExportAWBHeader)WrappedObject;

				if (awbHeader.IsDeleted)
				{
					throw new InvalidOperationException(ZString.Format("Cannot use ExportAWBHeader that has been deleted. The ExportAWBHeader {0} at first, and was used for wraping at {1}. Delete method stacktrace: {2}",
						this.IsExportAWBHeaderDeletedAtFirst == null
							? "IS NULL"
							: this.IsExportAWBHeaderDeletedAtFirst.Value
								? "IS DELETED"
								: "IS NOT DELETED",
						this.InstantiationTime,
						awbHeader.DeleteStackTrace));
				}

				return awbHeader;
			}
		}

		protected internal ShipmentExportAWBHeader ShipmentExportAWBHeader
		{
			get { return WrappedObject as ShipmentExportAWBHeader; }
		}

		public DocForwardingShipment DocShipment
		{
			get { return ShipmentExportAWBHeader != null ? DocForwardingShipment.New(ShipmentExportAWBHeader.Shipment, Factory) : null; }
		}

		ConsolExportAWBHeader ConsolExportAWBHeader
		{
			get { return WrappedObject as ConsolExportAWBHeader; }
		}

		public DocForwardingConsol DocConsol
		{
			get { return ConsolExportAWBHeader != null ? DocForwardingConsol.New(ConsolExportAWBHeader.Consol, Factory) : null; }
		}

		public DocAWB AWBForLabel
		{
			get { return awbForLabel ?? (awbForLabel = GetAWBForLabel()); }
		}
		DocAWB awbForLabel;

		DocAWB GetAWBForLabel()
		{
			DocAWB awb = this;

			if (WrappedObject is ShipmentExportAWBHeader)
			{
				var shipmentExportAWBHeader = WrappedObject as ShipmentExportAWBHeader;

				if (shipmentExportAWBHeader.ConsolForAWBLabel != null)
				{
					var consolHeader = shipmentExportAWBHeader.ConsolForAWBLabel.AWBHeader;
					awb = DocAWB.New(consolHeader, Factory);
				}
			}

			return awb;
		}

		public override string ToString()
		{
			return ReferenceNumber;
		}

		public override DocJobHeader JobHeader
		{
			get
			{
				if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House && ShipmentExportAWBHeader != null)
				{
					return DocJobHeader.New(ShipmentExportAWBHeader.InvoiceJobHeader, Factory);
				}

				return null;
			}
		}

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get
			{
				if (ShipmentExportAWBHeader != null)
				{
					return ShipmentExportAWBHeader.Shipment;
				}
				else if (ConsolExportAWBHeader != null)
				{
					return ConsolExportAWBHeader.Consol;
				}
				else
				{
					return base.BusinessObjectToLogAgainst;
				}
			}
		}

		protected override BusinessObject BusinessObjectForCustomFields => BusinessObjectToLogAgainst;

		#endregion

		#region Properties

		public ZBool PrintOptionalInformation
		{
			get { return ExportAWBHeader.PrintOptionalInformation; }
		}

		public ZBool IsCopy
		{
			get { return ReportName.ToLower().Contains((NoResString)"copy"); }
		}

		// Was used prior to 17 March 2008 in Aus only. Have now been removed. Leaving this here in case we need to add something back in. Still referenced in all AWB templates.
		public ZString AdditionalClause
		{
			get { return ZString.Empty; }
		}

		public ZString BillNumber
		{
			get { return ExportAWBHeader.EH_BillNumber; }
		}

		#region Shipper

		string[] SplitNameLines(ZString originalName)
		{
			string[] lines = new string[2];

			if (originalName.Length <= 20)
			{
				lines[0] = originalName;
				return lines;
			}

			int splitIndex = originalName.Substring(0, 20).LastIndexOf(' ');

			if (splitIndex >= 0)
			{
				lines[0] = originalName.Substring(0, splitIndex + 1);
				lines[1] = originalName.Substring(splitIndex + 1);
			}
			else
			{
				lines[0] = originalName.Substring(0, 20);
				lines[1] = originalName.Substring(20);
			}
			return lines;
		}

		ZString OriginalShipperName => ExportAWBHeader.EH_IsShipperOverriden
					? ExportAWBHeader.EH_ShipperOverride1
					: ExportAWBHeader.EH_ShipperName;

		public ZString ShipperNameLine1 => SplitNameLines(OriginalShipperName)[0];

		public ZString ShipperNameLine2 => SplitNameLines(OriginalShipperName)[1];

		public ZString ShipperAddress1
		{
			get { return ExportAWBHeader.EH_IsShipperOverriden ? ExportAWBHeader.EH_ShipperOverride1 : ExportAWBHeader.EH_ShipperName; }
		}

		public ZString ShipperAddress2
		{
			get { return ExportAWBHeader.EH_IsShipperOverriden ? ExportAWBHeader.EH_ShipperOverride2 : ExportAWBHeader.EH_ShipperAddress; }
		}

		public ZString ShipperAddress3
		{
			get { return ExportAWBHeader.EH_IsShipperOverriden ? ExportAWBHeader.EH_ShipperOverride3 : ExportAWBHeader.EH_ShipperAddress2; }
		}

		public ZString ShipperAddress4
		{
			get
			{
				return ExportAWBHeader.EH_IsShipperOverriden ? ExportAWBHeader.EH_ShipperOverride4
					: (ZString)(ShipperPlace + " " + ShipperState + " " + ExportAWBHeader.EH_ShipperPostCode + " " + ShipperCountryCode).Trim();
			}
		}

		public ZString ShipperAddress5
		{
			get
			{
				if (ExportAWBHeader.EH_IsShipperOverriden)
				{
					return ExportAWBHeader.EH_ShipperOverride5;
				}

				var stringsForAddress5 = new[] { ShipperContactCode, ShipperContactDetail, ExportAWBHeader.EH_ShipperContactName, ExtraShipperData };
				return ZString.Join(" ", stringsForAddress5.Where(x => !x.IsEmpty).ToArray()).Trim();
			}
		}

		#endregion

		#region Consignee

		ZString OriginalConsigneeName => ExportAWBHeader.EH_IsConsigneeOverriden
					? ExportAWBHeader.EH_ConsigneeOverride1
					: ExportAWBHeader.EH_ConsigneeName;

		public ZString ConsigneeNameLine1 => SplitNameLines(OriginalConsigneeName)[0];

		public ZString ConsigneeNameLine2 => SplitNameLines(OriginalConsigneeName)[1];

		public ZString ConsigneeAddress1
		{
			get { return ExportAWBHeader.EH_IsConsigneeOverriden ? ExportAWBHeader.EH_ConsigneeOverride1 : ExportAWBHeader.EH_ConsigneeName; }
		}

		public ZString ConsigneeAddress2
		{
			get { return ExportAWBHeader.EH_IsConsigneeOverriden ? ExportAWBHeader.EH_ConsigneeOverride2 : ExportAWBHeader.EH_ConsigneeAddress; }
		}

		public ZString ConsigneeAddress3
		{
			get { return ExportAWBHeader.EH_IsConsigneeOverriden ? ExportAWBHeader.EH_ConsigneeOverride3 : ExportAWBHeader.EH_ConsigneeAddress2; }
		}

		public ZString ConsigneeAddress4
		{
			get
			{
				return ExportAWBHeader.EH_IsConsigneeOverriden ? ExportAWBHeader.EH_ConsigneeOverride4
					: (ZString)(ConsigneePlace + " " + ConsigneeState + " " + ExportAWBHeader.EH_ConsigneePostCode + " " + ConsigneeCountryCode).Trim();
			}
		}

		public ZString ConsigneeAddress5
		{
			get
			{
				if (ExportAWBHeader.EH_IsConsigneeOverriden)
				{
					return ExportAWBHeader.EH_ConsigneeOverride5;
				}

				var stringsForAddress5 = new[] { ConsigneeContactCode, ConsigneeContactDetail, ExportAWBHeader.EH_ConsigneeContactName, RegistrationNumber };
				return ZString.Join(" ", stringsForAddress5.Where(x => !x.IsEmpty).ToArray()).Trim();
			}
		}

		#endregion

		public ZBool IsCarrierAWB
		{
			get { return ReportName.StartsWith((NoResString)"Carrier"); }
		}

		public ZBool UseChargeSet2
		{
			get
			{
				try
				{
#if DEBUG
					if (ForceUseChargeSet2IssueReporting)
					{
						throw new InvalidOperationException("You cannot access the DocWrapperContext properties until they have been setup by the Report. If you are accessing this property from the Constructor of your DocumentWrapper, try converting to a lazy loading pattern so that it gets accessed after it's been setup.");
					}
#endif

					return ReportName.StartsWith((NoResString)"Original 2") ||
								ReportName.StartsWith((NoResString)"Copy 4") ||
								ReportName.StartsWith((NoResString)"Copy 5") ||
								ReportName.StartsWith((NoResString)"Copy 6") ||
								ReportName.StartsWith((NoResString)"Neutral HAWB2") ||
								ReportName.StartsWith((NoResString)"Neutral MAWB2") ||
								ReportName.StartsWith((NoResString)"Carrier MAWB2");
				}
				catch (InvalidOperationException ex)
				{
					ReportUseChargeSet2Issue(ex);
					throw;
				}
			}
		}

		#region ErrorReporter UseChargeSet2 for WI00064568

		internal bool IsInitialized;

#if DEBUG
		internal bool ForceUseChargeSet2IssueReporting;
#endif

		void ReportUseChargeSet2Issue(InvalidOperationException ex)
		{
			#region SuppressResourceStringsCheckRegion

			string containsError = "You cannot access the DocWrapperContext properties until they have been setup by the Report. If you are accessing this property from the Constructor of your DocumentWrapper, try converting to a lazy loading pattern so that it gets accessed after it's been setup.";

			if (!ex.Message.Contains(containsError))
			{
				return;
			}

			string key = "WI00064568 International Logistics";
			string message = "Exception message:\r\n" + ex.Message + "\r\n\r\nStackTrace:\r\n" + ex.StackTrace;

			message += "\r\n\r\nDocAWB information:";

			var docWrapperContextmanager = Factory.GetDocWrapperContextManager();

			if (docWrapperContextmanager == null)
			{
				message += "\r\nFactory.GetDocWrapperContextManager() returns null";
			}
			else
			{
				if (docWrapperContextmanager.callstackWhenWrapperContextBecomeNull != null)
				{
					message += "\r\n\r\nStackTrace when context become null:\r\n";
					message += docWrapperContextmanager.callstackWhenWrapperContextBecomeNull.ToString();
				}

				message += "\r\nFactory.GetDocWrapperContextManager() returns NOT null";
			}

			message += "\r\nIsInitialized: " + IsInitialized;

			ErrorReporter.ReportOnce(key, message);

			#endregion
		}

		#endregion

		public ZString AlsoNotifyNameAndAddress
		{
			get
			{
				ZString result = string.Empty;
				result += AlsoNotifyName.Length > 0 ? AlsoNotifyName + "\n" : string.Empty;
				result += FormatAddress(AlsoNotifyAddress, AlsoNotifyPlace, AlsoNotifyState, AlsoNotifyPostCode, AlsoNotifyCountryCode, AlsoNotifyContactCode, AlsoNotifyContactDetail);
				return result;
			}
		}

		public ZBool UseOldStyleAWBFormat
		{
			get
			{
				ZBool result = false;
				if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House)
				{
					result = Env.Registry.Freight.AirWaybill.HAWBPaperType == Core.Constants.AWB.PaperTypes.IataOld;
				}
				else
				{
					result = Env.Registry.Freight.AirWaybill.MAWBPaperType == Core.Constants.AWB.PaperTypes.IataOld;
				}

				return result;
			}
		}

		protected ZString FormatAddress(ZString address, ZString city, ZString state, ZString postCode, ZString country, ZString contactCode, ZString contactDetail)
		{
			var result = new ZStringBuilder();

			result.AppendIfNotEmpty(address);
			result.AppendIfNotEmpty(city);
			result.AppendIfNotEmpty(state);
			result.AppendIfNotEmpty(postCode);
			result.AppendIfNotEmpty(country);
			result.AppendIfNotEmpty(contactCode);
			result.AppendIfNotEmpty(contactDetail);

			return result.ToStringWithDelimiterBetweenAppends(", ").TrimEnd(new char[] { ' ', '\n', ',' }).ToUpper();
		}

		#region Charges

		ZDecimal GetCODFeeTotalChargesForDirectShipment =>
			GetChargesForDirectShipment != null
			? new ZDecimal(GetChargesForDirectShipment
				.Where(x => x.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Destination && x.ChargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.Cod)
				.Sum(x => x.JR_LocalSellAmt))
			: ZDecimal.Zero;

		ZDecimal GetTotalChargesForDirectShipment(ZString chargeGroup) =>
			GetChargesForDirectShipment != null
			? new ZDecimal(GetChargesPerGroup(chargeGroup)
				.Sum(x => x.JR_LocalSellAmt))
			: ZDecimal.Zero;

		ZInt GetNumberOfChargesForDirectShipment(ZString chargeGroup) => GetChargesPerGroup(chargeGroup).Count();

		IEnumerable<JobCharge> GetChargesPerGroup(ZString chargeGroup) =>
			GetChargesForDirectShipment != null
			? GetChargesForDirectShipment.Where(x => x.ChargeCode.AC_ChargeGroup == chargeGroup &&
				!(x.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Destination && x.ChargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.Cod))
			: Enumerable.Empty<JobCharge>();

		IEnumerable<JobCharge> GetChargesForDirectShipment
		{
			get
			{
				ZGuid? jobPK;
				if (chargesForDirectShipment == null &&
					(ExportAWBHeader.Consol?.IsDirect ?? false) &&
					(jobPK = ExportAWBHeader.Consol?.Shipments.Cast<ForwardingShipment>().FirstOrDefault()?.ShipmentJobHeader?.PK).HasValue)
				{
					chargesForDirectShipment = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobPK));
				}
				return chargesForDirectShipment;
			}
		}
		IEnumerable<JobCharge> chargesForDirectShipment;

#if DEBUG

		public void DebugOnlySetChargesForDirectShipmentToNull()
		{
			chargesForDirectShipment = null;
		}
#endif

		IEnumerable<JobCharge> GetOtherChargesForDirectShipment
		{
			get
			{
				var excludedChargeGroups = new[]
				{
					ChargeCodeGroupList.Codes.Loading,
					ChargeCodeGroupList.Codes.Unloading,
					ChargeCodeGroupList.Codes.Origin,
					ChargeCodeGroupList.Codes.Destination
				};
				var charges = GetChargesForDirectShipment;

				return charges != null
					? charges.Where(x => !excludedChargeGroups.Contains(x.ChargeCode.AC_ChargeGroup.ToString()))
					: Enumerable.Empty<JobCharge>();
			}
		}

		public ZString OtherChargesWithDescriptionForDirectShipment
		{
			get
			{
				ZString result = ZString.Empty;
				if (ExportAWBHeader.Consol != null)
				{
					if (ExportAWBHeader.Consol.IsDirect)
					{
						if (OtherChargesForDirectShipment != ZDecimal.Zero)
						{
							result = OtherChargesForDirectShipment.ToString() + OtherChargesDescriptionForDirectShipment;
						}
					}
					else
					{
						result = OtherCharges1;
					}
				}
				return result;
			}
		}

		public ZDecimal OtherChargesForDirectShipment => GetOtherChargesForDirectShipment.Sum(x => x.JR_LocalSellAmt);

		public ZString OtherChargesDescriptionForDirectShipment =>
			GetOtherChargesForDirectShipment.Any()
			? GetOtherChargesForDirectShipment.Count() == 1
				? GetOtherChargesForDirectShipment.First().JR_Desc
				: new ZString(Res.GetString("77352b03-6b5f-446f-9b42-ae348f368949", "Miscellaneous"))
			: ZString.Empty;

		public ZDecimal PickupChargesForDirectShipment => GetTotalChargesForDirectShipment(ChargeCodeGroupList.Codes.Loading);

		public ZDecimal DeliveryChargesForDirectShipment => GetTotalChargesForDirectShipment(ChargeCodeGroupList.Codes.Unloading);

		public ZDecimal OriginAdvancedChargesForDirectShipment => GetTotalChargesForDirectShipment(ChargeCodeGroupList.Codes.Origin);

		public ZString DescriptionOfOriginAdvance =>
			GetChargesPerGroup(ChargeCodeGroupList.Codes.Origin).Any()
			? GetNumberOfChargesForDirectShipment(ChargeCodeGroupList.Codes.Origin) == 1
				? GetChargesPerGroup(ChargeCodeGroupList.Codes.Origin).First().JR_Desc
				: new ZString(Res.GetString("8459a182-7a9e-457b-8ae3-3ae822568116", "Miscellaneous"))
			: ZString.Empty;

		public ZDecimal DestinationAdvancedChargesForDirectShipment => GetTotalChargesForDirectShipment(ChargeCodeGroupList.Codes.Destination);

		public ZString DescriptionOfDestinationAdvance =>
			GetChargesPerGroup(ChargeCodeGroupList.Codes.Destination).Any()
			? GetNumberOfChargesForDirectShipment(ChargeCodeGroupList.Codes.Destination) == 1
				? GetChargesPerGroup(ChargeCodeGroupList.Codes.Destination).First().JR_Desc
				: new ZString(Res.GetString("b8434598-c6ae-411e-b784-93534c17805b", "Miscellaneous"))
			: ZString.Empty;

		public ZDecimal CODFeeForDirectShipment
		{
			get { return GetCODFeeTotalChargesForDirectShipment; }
		}

		public ZDecimal ShippersCODForDirectShipment
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (ExportAWBHeader.Consol != null)
				{
					if (ExportAWBHeader.Consol.IsDirect)
					{
						if (ExportAWBHeader.Consol.Shipments.Count > 0 && ExportAWBHeader.Consol.Shipments[0].IsCollect)
						{
							result = ExportAWBHeader.Consol.Shipments[0].JS_ShipperCODAmount;
						}
					}
				}
				return result;
			}
		}

		public ZString OtherCharges1
		{
			get { return FormatOtherCharges(ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString OtherCharges2
		{
			get { return FormatOtherCharges(ExportAWBHeader.EH_AsAgreed2nd); }
		}

		readonly ZString AsAgreedText = (NoResString)"As Agreed";
		readonly int MaximumNumberOfOtherChargesLines = 5;
		readonly int MaximumTotalWidthOfOtherChargesLines = 60;

		ZString FormatOtherCharges(ZString asAgreed)
		{
			var otherCharges = ExportAWBHeader.AWBOtherCharges.Cast<ExportAWBOtherCharges>();

			if (otherCharges.Any()
				&& otherCharges.All(otherCharge => !otherCharge.EO_ChargeCode.IsEmpty)
				&& (ExportAWBHeader.AWBType != ExportAWBHeader.TypeOfAWB.House || Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen))
			{
				var result = FormatOtherChargesPreservingDescriptions(asAgreed);
				if (result.IsEmpty)
				{
					result = FormatOtherChargesAsCodeAmountTable(asAgreed);
				}

				return result.ToUpper();
			}

			return FormatOtherChargesTruncatingLongDescriptions(asAgreed);
		}

		int GetOtherChargesAmountMaxLength(ZString asAgreed)
		{
			if (ExportAWBHeader.AWBOtherCharges.Count == 0)
			{
				return 0;
			}

			if (asAgreed == Core.Constants.AWB.AsAgreedTypes.Codes.All)
			{
				return AsAgreedText.Length;
			}

			var otherChargesCollection = ExportAWBHeader.AWBOtherCharges.Cast<ExportAWBOtherCharges>();

			if (asAgreed == Core.Constants.AWB.AsAgreedTypes.Codes.None)
			{
				return otherChargesCollection.Max(x => x.EO_Amount.ToString(DefaultCurrencyMoneyDecimals).Length);
			}

			var asAgreedLength = otherChargesCollection.Any(x => IsPaymentAsAgreed(x.EO_PPDCLT, asAgreed))
								 ? AsAgreedText.Length
								 : 0;

			var excludeAsAgreedCharges = otherChargesCollection.Where(x => !IsPaymentAsAgreed(x.EO_PPDCLT, asAgreed));
			var longestChargeLength = excludeAsAgreedCharges.Any()
									? excludeAsAgreedCharges.Max(x => x.EO_Amount.ToString(DefaultCurrencyMoneyDecimals).Length)
									: 0;

			return Math.Max(longestChargeLength, asAgreedLength);
		}

		ZString FormatOtherChargesPreservingDescriptions(ZString asAgreed)
		{
			var codes = new List<string>();
			var descriptions = new List<string>();
			var amounts = new List<string>();

			int maxLengthOfAmount = GetOtherChargesAmountMaxLength(asAgreed);

			foreach (ExportAWBOtherCharges otherCharges in ExportAWBHeader.AWBOtherCharges)
			{
				var isAsAgreed = IsPaymentAsAgreed(otherCharges.EO_PPDCLT, asAgreed);
				var amountAsString = isAsAgreed
					? AsAgreedText
					: FormatMoneyValue(otherCharges.EO_Amount, maxLengthOfAmount);

				codes.Add(otherCharges.EO_ChargeCode + otherCharges.EO_EntitlementCode);
				descriptions.Add(otherCharges.EO_ChargeDescription);
				amounts.Add(amountAsString);
			}

			int codesWidth = 3;
			int amountsWidth = maxLengthOfAmount;

			int totalPaddingWidth = 2;
			int descriptionsWidth = MaximumTotalWidthOfOtherChargesLines - codesWidth - amountsWidth - totalPaddingWidth;

			var columns = new FormatColumn[]
			{
				new FormatColumn(codes, codesWidth)
				{
				},

				new FormatColumn(descriptions, descriptionsWidth)
				{
					LeftPadding = 1,
				},

				new FormatColumn(amounts, amountsWidth)
				{
					LeftPadding = 1,
					Options = FormatColumnOptions.RightAlign,
				},
			};

			var table = new FormatTable(false, columns);
			var tableLines = table.Body.ToArray();

			if (tableLines.Length <= MaximumNumberOfOtherChargesLines)
			{
				var builder = new ZStringBuilder(tableLines);
				return builder.ToStringWithNewLineBetweenAppends();
			}

			return ZString.Empty;
		}

		ZString FormatOtherChargesAsCodeAmountTable(ZString asAgreed)
		{
			var result = ZString.Empty;
			var maxLengthOfAmount = GetOtherChargesAmountMaxLength(asAgreed);

			var counter = 0;
			var numberOfColumns = 3;

			foreach (ExportAWBOtherCharges otherCharges in ExportAWBHeader.AWBOtherCharges)
			{
				var isAsAgreed = IsPaymentAsAgreed(otherCharges.EO_PPDCLT, asAgreed);
				var amountAsString = isAsAgreed ? AsAgreedText : FormatMoneyValue(otherCharges.EO_Amount, maxLengthOfAmount);
				var currentChargeAsString = otherCharges.EO_ChargeCode + otherCharges.EO_EntitlementCode + " " + amountAsString + "   ";

				result += currentChargeAsString;
				if (++counter % numberOfColumns == 0)
				{
					result += System.Environment.NewLine;
				}
			}

			return result;
		}

		ZString FormatOtherChargesTruncatingLongDescriptions(ZString asAgreed)
		{
			ZString result = ZString.Empty;
			int maxLengthOfAmount = GetOtherChargesAmountMaxLength(asAgreed);

			if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House)
			{
				var codesAndPaddingWidth = 4;

				var maxLengthOfDescription = ExportAWBHeader.AWBOtherCharges.Count <= MaximumNumberOfOtherChargesLines
					? MaximumTotalWidthOfOtherChargesLines - maxLengthOfAmount - codesAndPaddingWidth
					: (MaximumTotalWidthOfOtherChargesLines - 2 * (maxLengthOfAmount + codesAndPaddingWidth)) / 2;

				var otherChargesStrings = new List<string>();
				foreach (ExportAWBOtherCharges otherCharges in ExportAWBHeader.AWBOtherCharges)
				{
					var isAsAgreed = IsPaymentAsAgreed(otherCharges.EO_PPDCLT, asAgreed);
					var amountAsString = isAsAgreed ? AsAgreedText : FormatMoneyValue(otherCharges.EO_Amount, maxLengthOfAmount);
					otherChargesStrings.Add(otherCharges.EO_EntitlementCode + " " + otherCharges.EO_ChargeDescription.SubstringSafe(0, maxLengthOfDescription).PadRight(maxLengthOfDescription, ' ') + " " + amountAsString);
				}

				for (int i = 0; i < MaximumNumberOfOtherChargesLines; i++)
				{
					if (i < otherChargesStrings.Count)
					{
						result += otherChargesStrings[i];

						var secondColumnIndex = i + MaximumNumberOfOtherChargesLines;
						if (secondColumnIndex < otherChargesStrings.Count)
						{
							result += "   " + otherChargesStrings[secondColumnIndex];
						}

						result += System.Environment.NewLine;
					}
				}
			}
			else
			{
				var codesAndPaddingWidth = 8;
				var maxLengthOfDescription = MaximumTotalWidthOfOtherChargesLines - maxLengthOfAmount - codesAndPaddingWidth;

				foreach (ExportAWBOtherCharges otherCharges in ExportAWBHeader.AWBOtherCharges)
				{
					var isAsAgreed = IsPaymentAsAgreed(otherCharges.EO_PPDCLT, asAgreed);
					var amountAsString = isAsAgreed ? AsAgreedText : FormatMoneyValue(otherCharges.EO_Amount);
					result += otherCharges.EO_ChargeCode + otherCharges.EO_EntitlementCode + " " + otherCharges.EO_ChargeDescription.SubstringSafe(0, maxLengthOfDescription) + " " + amountAsString + System.Environment.NewLine;
				}
			}

			return result.ToUpper();
		}

		#endregion

		#region MAWB Status

		public ZString MAWBStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (ExportAWBHeader.IsReprintingNeutralMAWB)
				{
					if (FreightDataRegistry.Instance.IncludeReprintStatusOnMAWBReprints.Value)
					{
						result = "REPRINT";
					}
				}
				else if (ExportAWBHeader.IsPrintingDraftNeutralMAWB)
				{
					result = "DRAFT";
				}

				return result;
			}
		}

		#endregion

		protected int FirstEmptyRateLine
		{
			get
			{
				int result = -1;
				for (int i = 0; i < ExportAWBHeader.AWBRateLines.Count; i++)
				{
					ExportAWBRateLine rateLine = ExportAWBHeader.AWBRateLines[i];
					if (result == -1 && rateLine.IsRateDescriptionEmpty)
					{
						result = i;
					}
					else if (!rateLine.IsRateDescriptionEmpty)
					{
						result = -1;
					}
				}

				return result;
			}
		}

		#region NoPieces

		public ZString NoPieces(int i)
		{
			ExportAWBRateLine rateLine = ExportAWBHeader.AWBRateLines[i];
			return !rateLine.ER_NoOfPiecesOrRCP.IsEmpty && rateLine.ER_NoOfPiecesOrRCP != "0" ? rateLine.ER_NoOfPiecesOrRCP.ToString() : string.Empty;
		}

		public ZString NoPieces1
		{
			get { return NoPieces(0); }
		}

		public ZString NoPieces2
		{
			get { return NoPieces(1); }
		}

		public ZString NoPieces3
		{
			get { return NoPieces(2); }
		}

		public ZString NoPieces4
		{
			get { return NoPieces(3); }
		}

		public ZString NoPieces5
		{
			get { return NoPieces(4); }
		}

		public ZString NoPieces6
		{
			get { return NoPieces(5); }
		}

		public ZString NoPieces7
		{
			get { return NoPieces(6); }
		}

		public ZString NoPieces8
		{
			get { return NoPieces(7); }
		}

		public ZString NoPieces9
		{
			get { return NoPieces(8); }
		}

		public ZString NoPieces10
		{
			get { return NoPieces(9); }
		}

		public ZString NoPieces11
		{
			get { return NoPieces(10); }
		}

		#endregion

		#region RateLineText

		const int NumberOfAWBRatelineOvertypedNotes = 10;
		const int MaxNumberOfCharactersPerRateLine = 67;

		protected ZString[] RateLinesText;
		protected ZString RateLineText(int i)
		{
			if (FirstEmptyRateLine > 0)
			{
				i -= (FirstEmptyRateLine - 1);
			}
			ZString result = string.Empty;
			if (RateLinesText == null)
			{
				RateLinesText = PopulateRateLinesText();
			}

			if (RateLinesText.Length > i && i >= 0)
			{
				result = RateLinesText[i].TrimEnd();
			}

			return result;
		}

		ZString[] PopulateRateLinesText()
		{
			var textSection = new TextSection(MaxNumberOfCharactersPerRateLine);

			foreach (var text in ExportAWBHeader.AWBRatelineOvertypedNotes.ToString().TrimEnd().Split(new[] { System.Environment.NewLine }, StringSplitOptions.None))
			{
				textSection.Add(text);
			}

			if (textSection.Height > NumberOfAWBRatelineOvertypedNotes)
			{
				var emptyLines = textSection.Where(x => string.IsNullOrWhiteSpace(x.ToString())).ToList();

				while (emptyLines.Any() && textSection.Height > NumberOfAWBRatelineOvertypedNotes)
				{
					var emptyLine = emptyLines.First();

					textSection.Remove(emptyLine);
					emptyLines.Remove(emptyLine);
				}
			}

			var lines = textSection.ToStringArray();

			if (lines.Length > NumberOfAWBRatelineOvertypedNotes)
			{
				var lastLineIndex = NumberOfAWBRatelineOvertypedNotes - 1;
				var overflownLines = ZString.Join(" ", lines, NumberOfAWBRatelineOvertypedNotes, lines.Length - NumberOfAWBRatelineOvertypedNotes);
				lines[lastLineIndex] += " " + overflownLines;
				lines[lastLineIndex] = lines[lastLineIndex].SubstringSafe(0, MaxNumberOfCharactersPerRateLine);
			}

			return lines;
		}

		public ZString RateLine2Text
		{
			get { return RateLineText(0); }
		}

		public ZString RateLine3Text
		{
			get { return RateLineText(1); }
		}

		public ZString RateLine4Text
		{
			get { return RateLineText(2); }
		}

		public ZString RateLine5Text
		{
			get { return RateLineText(3); }
		}

		public ZString RateLine6Text
		{
			get { return RateLineText(4); }
		}

		public ZString RateLine7Text
		{
			get { return RateLineText(5); }
		}

		public ZString RateLine8Text
		{
			get { return RateLineText(6); }
		}

		public ZString RateLine9Text
		{
			get { return RateLineText(7); }
		}

		public ZString RateLine10Text
		{
			get { return RateLineText(8); }
		}

		public ZString RateLine11Text
		{
			get { return RateLineText(9); }
		}

		#endregion

		#region GrossWeight

		public ZString GrossWeight(int i)
		{
			ExportAWBRateLine rateLine = ExportAWBHeader.AWBRateLines[i];
			int decimalPlaces = rateLine.GetNumberOfDecimals(rateLine.ER_GrossWeightInfo);
			return rateLine.ER_GrossWeight != 0M ? rateLine.ER_GrossWeight.ToString(decimalPlaces) : string.Empty;
		}

		public ZString GrossWeight1
		{
			get { return GrossWeight(0); }
		}

		public ZString GrossWeight2
		{
			get { return GrossWeight(1); }
		}

		public ZString GrossWeight3
		{
			get { return GrossWeight(2); }
		}

		public ZString GrossWeight4
		{
			get { return GrossWeight(3); }
		}

		public ZString GrossWeight5
		{
			get { return GrossWeight(4); }
		}

		public ZString GrossWeight6
		{
			get { return GrossWeight(5); }
		}

		public ZString GrossWeight7
		{
			get { return GrossWeight(6); }
		}

		public ZString GrossWeight8
		{
			get { return GrossWeight(7); }
		}

		public ZString GrossWeight9
		{
			get { return GrossWeight(8); }
		}

		public ZString GrossWeight10
		{
			get { return GrossWeight(9); }
		}

		public ZString GrossWeight11
		{
			get { return GrossWeight(10); }
		}

		#endregion

		#region RateUQ

		public ZString RateUQ(int i)
		{
			ExportAWBRateLine rateLine = ExportAWBHeader.AWBRateLines[i];
			return rateLine.ER_WeightInLBsOrKGs;
		}

		public ZString RateUQ1
		{
			get { return RateUQ(0); }
		}

		public ZString RateUQ2
		{
			get { return RateUQ(1); }
		}

		public ZString RateUQ3
		{
			get { return RateUQ(2); }
		}

		public ZString RateUQ4
		{
			get { return RateUQ(3); }
		}

		public ZString RateUQ5
		{
			get { return RateUQ(4); }
		}

		public ZString RateUQ6
		{
			get { return RateUQ(5); }
		}

		public ZString RateUQ7
		{
			get { return RateUQ(6); }
		}

		public ZString RateUQ8
		{
			get { return RateUQ(7); }
		}

		public ZString RateUQ9
		{
			get { return RateUQ(8); }
		}

		public ZString RateUQ10
		{
			get { return RateUQ(9); }
		}

		public ZString RateUQ11
		{
			get { return RateUQ(10); }
		}

		#endregion

		#region RateClass

		public ZString RateClass(int i)
		{
			ExportAWBRateLine rateLine = ExportAWBHeader.AWBRateLines[i];
			return rateLine.ER_RateClass;
		}

		public ZString RateClass1
		{
			get { return RateClass(0); }
		}

		public ZString RateClass2
		{
			get { return RateClass(1); }
		}

		public ZString RateClass3
		{
			get { return RateClass(2); }
		}

		public ZString RateClass4
		{
			get { return RateClass(3); }
		}

		public ZString RateClass5
		{
			get { return RateClass(4); }
		}

		public ZString RateClass6
		{
			get { return RateClass(5); }
		}

		public ZString RateClass7
		{
			get { return RateClass(6); }
		}

		public ZString RateClass8
		{
			get { return RateClass(7); }
		}

		public ZString RateClass9
		{
			get { return RateClass(8); }
		}

		public ZString RateClass10
		{
			get { return RateClass(9); }
		}

		public ZString RateClass11
		{
			get { return RateClass(10); }
		}

		#endregion

		#region CommodityItemNumber

		public ZString CommodityItemNumber(int i)
		{
			ExportAWBRateLine rateLine = ExportAWBHeader.AWBRateLines[i];
			return rateLine.ER_CommodityItemNumber.ToUpper();
		}

		public ZString CommodityItemNumber1
		{
			get { return CommodityItemNumber(0); }
		}

		public ZString CommodityItemNumber2
		{
			get { return CommodityItemNumber(1); }
		}

		public ZString CommodityItemNumber3
		{
			get { return CommodityItemNumber(2); }
		}

		public ZString CommodityItemNumber4
		{
			get { return CommodityItemNumber(3); }
		}

		public ZString CommodityItemNumber5
		{
			get { return CommodityItemNumber(4); }
		}

		public ZString CommodityItemNumber6
		{
			get { return CommodityItemNumber(5); }
		}

		public ZString CommodityItemNumber7
		{
			get { return CommodityItemNumber(6); }
		}

		public ZString CommodityItemNumber8
		{
			get { return CommodityItemNumber(7); }
		}

		public ZString CommodityItemNumber9
		{
			get { return CommodityItemNumber(8); }
		}

		public ZString CommodityItemNumber10
		{
			get { return CommodityItemNumber(9); }
		}

		public ZString CommodityItemNumber11
		{
			get { return CommodityItemNumber(10); }
		}

		#endregion

		#region ChargeableWeight

		public ZString ChargeableWeight(int i)
		{
			ExportAWBRateLine rateLine = ExportAWBHeader.AWBRateLines[i];
			return rateLine.ER_ChargeableWeight != 0M ? rateLine.ER_ChargeableWeight.ToString(1) : string.Empty;
		}

		public ZString ChargeableWeight1
		{
			get { return ChargeableWeight(0); }
		}

		public ZString ChargeableWeight2
		{
			get { return ChargeableWeight(1); }
		}

		public ZString ChargeableWeight3
		{
			get { return ChargeableWeight(2); }
		}

		public ZString ChargeableWeight4
		{
			get { return ChargeableWeight(3); }
		}

		public ZString ChargeableWeight5
		{
			get { return ChargeableWeight(4); }
		}

		public ZString ChargeableWeight6
		{
			get { return ChargeableWeight(5); }
		}

		public ZString ChargeableWeight7
		{
			get { return ChargeableWeight(6); }
		}

		public ZString ChargeableWeight8
		{
			get { return ChargeableWeight(7); }
		}

		public ZString ChargeableWeight9
		{
			get { return ChargeableWeight(8); }
		}

		public ZString ChargeableWeight10
		{
			get { return ChargeableWeight(9); }
		}

		public ZString ChargeableWeight11
		{
			get { return ChargeableWeight(10); }
		}

		#endregion

		#region RateCharge

		public ZString RateCharge(int i, ZString asAgreed)
		{
			var rateLine = ExportAWBHeader.AWBRateLines[i];
			var isAsAgreed = IsRateLineAsAgreed(rateLine, asAgreed);

			return !isAsAgreed && rateLine.ER_RateChargeOrDiscount > 0M
				? FormatMoneyValue(rateLine.ER_RateChargeOrDiscount)
				: ZString.Empty;
		}

		public ZString RateCharge1_1
		{
			get { return RateCharge(0, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_2
		{
			get { return RateCharge(1, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_3
		{
			get { return RateCharge(2, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_4
		{
			get { return RateCharge(3, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_5
		{
			get { return RateCharge(4, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_6
		{
			get { return RateCharge(5, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_7
		{
			get { return RateCharge(6, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_8
		{
			get { return RateCharge(7, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_9
		{
			get { return RateCharge(8, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_10
		{
			get { return RateCharge(9, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge1_11
		{
			get { return RateCharge(10, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateCharge2_1
		{
			get { return RateCharge(0, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_2
		{
			get { return RateCharge(1, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_3
		{
			get { return RateCharge(2, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_4
		{
			get { return RateCharge(3, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_5
		{
			get { return RateCharge(4, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_6
		{
			get { return RateCharge(5, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_7
		{
			get { return RateCharge(6, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_8
		{
			get { return RateCharge(7, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_9
		{
			get { return RateCharge(8, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_10
		{
			get { return RateCharge(9, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateCharge2_11
		{
			get { return RateCharge(10, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		#endregion

		#region RateLineTotal

		public ZString RateLineTotal(int i, ZString asAgreed)
		{
			if (asAgreed == Core.Constants.AWB.AsAgreedTypes.Codes.All)
			{
				return i == 0 ? AsAgreedText : ZString.Empty;
			}

			var rateLine = ExportAWBHeader.AWBRateLines[i];
			var isAsAgreed = IsRateLineAsAgreed(rateLine, asAgreed);

			if (isAsAgreed && rateLine.ER_Total != 0M)
			{
				return AsAgreedText;
			}

			return rateLine.ER_Total != 0M
				? FormatMoneyValue(rateLine.ER_Total)
				: ZString.Empty;
		}

		public ZString RateLineTotal1_1
		{
			get { return RateLineTotal(0, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_2
		{
			get { return RateLineTotal(1, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_3
		{
			get { return RateLineTotal(2, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_4
		{
			get { return RateLineTotal(3, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_5
		{
			get { return RateLineTotal(4, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_6
		{
			get { return RateLineTotal(5, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_7
		{
			get { return RateLineTotal(6, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_8
		{
			get { return RateLineTotal(7, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_9
		{
			get { return RateLineTotal(8, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_10
		{
			get { return RateLineTotal(9, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal1_11
		{
			get { return RateLineTotal(10, ExportAWBHeader.EH_AsAgreed1st); }
		}

		public ZString RateLineTotal2_1
		{
			get { return RateLineTotal(0, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_2
		{
			get { return RateLineTotal(1, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_3
		{
			get { return RateLineTotal(2, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_4
		{
			get { return RateLineTotal(3, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_5
		{
			get { return RateLineTotal(4, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_6
		{
			get { return RateLineTotal(5, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_7
		{
			get { return RateLineTotal(6, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_8
		{
			get { return RateLineTotal(7, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_9
		{
			get { return RateLineTotal(8, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_10
		{
			get { return RateLineTotal(9, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		public ZString RateLineTotal2_11
		{
			get { return RateLineTotal(10, ExportAWBHeader.EH_AsAgreed2nd); }
		}

		#endregion

		#region NatureAndQuantityOfGoods

		public ZString NatureAndQuantityOfGoods1
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine1);
				}
				else
				{
					result = Res.GetString("c19fd220-6884-4c05-8c6f-ba03366a9848", "See Attached Follow On Page");
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods2
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine2);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods3
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine3);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods4
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine4);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods5
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine5);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods6
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine6);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods7
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine7);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods8
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine8);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods9
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine9);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods10
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine10);
				}
				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods11
		{
			get
			{
				ZString result = ZString.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine11);
				}

				return result;
			}
		}

		public ZString NatureAndQuantityOfGoods12
		{
			get
			{
				ZString result = ZString.Empty;
				if (!HasFollowOnPage)
				{
					result = GetDescriptionFromAWBRateLine(ExportAWBHeader.AWBRateLine12);
				}

				return result;
			}
		}

		public ZString ExtraNatureAndQtyOfGoods
		{
			get
			{
				ZString result = string.Empty;
				if (!HasFollowOnPage)
				{
					result = NatureAndQuantityOfGoods11 + "\n" + NatureAndQuantityOfGoods12;
				}
				return result;
			}
		}

		ZString GetDescriptionFromAWBRateLine(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine currentRateLine)
		{
			var master = currentRateLine.Master;
			var lineCount = currentRateLine.ER_LineCount;
			for (var i = 0; i < lineCount; i++)
			{
				var rateLine = master.GetRateLineAt(lineCount - i) as Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine;
				if (IsLithiumBattery(rateLine) && rateLine.NatureAndQtyOfGoodsLithiumBattery.WrappedDescriptions.Count > i)
				{
					return rateLine.NatureAndQtyOfGoodsLithiumBattery.WrappedDescriptions[i];
				}
			}

			return currentRateLine.NatureAndQtyOfGoodsDescription;
		}

		bool IsLithiumBattery(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine rateLine)
		{
			return rateLine != null && rateLine.NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
		}

		ZBool HasFollowOnPage
		{
			get { return MenuTitle.Contains((NoResString)"Follow on"); }
		}

		public ZString DetailedGoodsDescription
		{
			get
			{
				ZString result = string.Empty;
				if (ShipmentExportAWBHeader != null && ShipmentExportAWBHeader.Shipment != null)
				{
					result += ShipmentExportAWBHeader.Shipment.DetailedGoodsDescriptionNoteText;

					if (!ShipmentExportAWBHeader.Shipment.JS_MarksAndNumbers.IsEmpty)
					{
						result += "\n" + ShipmentExportAWBHeader.Shipment.JS_MarksAndNumbers;
					}

					if (HasFollowOnPage)
					{
						result += "\n" + ShipmentExportAWBHeader.VolumeAndDimensionForFollowOnPage;

						if (!this.ShipmentExportAWBHeader.Shipment.ExportStatement.IsEmpty)
						{
							result += "\n" + this.ShipmentExportAWBHeader.Shipment.ExportStatement;
						}

						if (ExportAWBHeader.OriginCountry != null)
						{
							foreach (ExportStatementSetting mandatorySetting in FreightDataRegistry.Instance.ExportStatementSettings.Value.GetMandatoryStatements(ExportAWBHeader.OriginCountry.Code.Left(2)))
							{
								if (!mandatorySetting.Statement.IsEmpty)
								{
									if ((mandatorySetting.UseOnHawb && ShipmentExportAWBHeader != null)
										|| ((mandatorySetting.UseOnConsolidationMawb
										|| mandatorySetting.UseOnDirectIATAMawb) && ShipmentExportAWBHeader == null))
									{
										result += "\n" + mandatorySetting.Statement;
									}
								}
							}
						}
					}
				}

				return result.Trim();
			}
		}

		public ZString ExtraGoodsDescription
		{
			get
			{
				if (extraGoodsDescription == ZString.Empty)
				{
					Action<ZString> stripGoodsDescription = goodsDescriptionToStrip =>
					{
						if (extraGoodsDescription.StartsWith(goodsDescriptionToStrip))
						{
							extraGoodsDescription = extraGoodsDescription.Substring(goodsDescriptionToStrip.Length);
							extraGoodsDescription = extraGoodsDescription.Trim();
						}
					};

					extraGoodsDescription = DetailedGoodsDescription;

					stripGoodsDescription(NatureAndQuantityOfGoods1);
					stripGoodsDescription(NatureAndQuantityOfGoods2);
					stripGoodsDescription(NatureAndQuantityOfGoods3);
					stripGoodsDescription(NatureAndQuantityOfGoods4);
					stripGoodsDescription(NatureAndQuantityOfGoods5);
					stripGoodsDescription(NatureAndQuantityOfGoods6);
					stripGoodsDescription(NatureAndQuantityOfGoods7);
					stripGoodsDescription(NatureAndQuantityOfGoods8);
					stripGoodsDescription(NatureAndQuantityOfGoods9);
					stripGoodsDescription(NatureAndQuantityOfGoods10);
					stripGoodsDescription(NatureAndQuantityOfGoods11);
					stripGoodsDescription(NatureAndQuantityOfGoods12);
				}

				return extraGoodsDescription;
			}
		}
		ZString extraGoodsDescription;

		public DocCountry OriginCountry
		{
			get { return DocCountry.New(ExportAWBHeader.OriginCountry, Factory); }
		}

		#endregion

		#region AccountingInformation

		public virtual ZString AccountingInformation
		{
			get
			{
				var parts = new[]
				{
					AccountingInformation1,
					AccountingInformation2,
					AccountingInformation3,
					AccountingInformation4,
					AccountingInformation5
				};

				return ZString.Join("\n", parts.Where(p => !p.IsEmpty).ToArray()).ToUpper().Trim();
			}
		}

		bool GetAccountingInfo(int lineNumber, out ZString result)
		{
			var informations = ExportAWBHeader.AWBAccountingInformations.Cast<ExportAWBAccountingInformation>().Where(info => !info.IsItalianRegistrationCode).ToList();

			if (informations.Count >= lineNumber)
			{
				ExportAWBAccountingInformation info = informations[lineNumber - 1];
				result = (info.EA_InformationID.Length > 0 ? info.EA_InformationID + " " : string.Empty) + info.EA_Information + "\n";
			}
			else
			{
				result = string.Empty;
			}

			return informations.Count > 0;
		}

		public virtual ZString AccountingInformation1
		{
			get
			{
				ZString result;

				if (!GetAccountingInfo(1, out result))
				{
					if (ExportAWBHeader.EH_IsNotifyOverriden)
					{
						result = ExportAWBHeader.EH_NotifyOverride1;
					}
					else if (ExportAWBHeader.EH_AlsoNotifyName != string.Empty)
					{
						result = (NoResString)"Also Notify: " + ExportAWBHeader.EH_AlsoNotifyName;
					}
				}

				return result.ToUpper().Trim();
			}
		}

		public virtual ZString AccountingInformation2
		{
			get
			{
				ZString result;

				if (!GetAccountingInfo(2, out result))
				{
					if (ExportAWBHeader.EH_IsNotifyOverriden)
					{
						result = ExportAWBHeader.EH_NotifyOverride2;
					}
					else
					{
						result = ExportAWBHeader.EH_AlsoNotifyAddress;
					}
				}

				return result.ToUpper().Trim();
			}
		}

		public virtual ZString AccountingInformation3
		{
			get
			{
				ZString result;

				if (!GetAccountingInfo(3, out result))
				{
					if (ExportAWBHeader.EH_IsNotifyOverriden)
					{
						result = ExportAWBHeader.EH_NotifyOverride3;
					}
					else
					{
						result = ExportAWBHeader.EH_AlsoNotifyPlace + " " + ExportAWBHeader.EH_AlsoNotifyState + " " + ExportAWBHeader.EH_AlsoNotifyPostCode + " " + ExportAWBHeader.EH_AlsoNotifyCountryCode;
					}
				}

				return result.ToUpper().Trim();
			}
		}

		public virtual ZString AccountingInformation4
		{
			get
			{
				ZString result;

				if (!GetAccountingInfo(4, out result))
				{
					if (ExportAWBHeader.EH_IsNotifyOverriden)
					{
						result = ExportAWBHeader.EH_NotifyOverride4;
					}
					else
					{
						var stringsForAddress4 = new[] { AlsoNotifyContactCode, AlsoNotifyContactDetail, ExportAWBHeader.EH_AlsoNotifyContactName };
						result = ZString.Join(" ", stringsForAddress4.Where(x => !x.IsEmpty).ToArray());
					}
				}

				return result.ToUpper().Trim();
			}
		}

		public virtual ZString AccountingInformation5
		{
			get
			{
				ZString result;

				if (!GetAccountingInfo(5, out result))
				{
					if (ExportAWBHeader.EH_IsNotifyOverriden)
					{
						result = ExportAWBHeader.EH_NotifyOverride5;
					}

					result = result + " " + ExportAWBHeader.ExtraAlsoNotifyData;
				}

				return result.ToUpper().Trim();
			}
		}

		#endregion

		public ZInt ItemsPrepaid
		{
			get
			{
				ZInt result = ZInt.Zero;
				if (ExportAWBHeader.Consol != null)
				{
					foreach (ForwardingShipment currentShippment in ExportAWBHeader.Consol.Shipments)
					{
						if (currentShippment.IsPrepaid)
						{
							result += currentShippment.JS_OuterPacks;
						}
					}
				}
				return result;
			}
		}

		public ZInt ItemsCollect
		{
			get
			{
				ZInt result = ZInt.Zero;
				if (ExportAWBHeader.Consol != null)
				{
					foreach (ForwardingShipment currentShippment in ExportAWBHeader.Consol.Shipments)
					{
						if (currentShippment.IsCollect)
						{
							result += currentShippment.JS_OuterPacks;
						}
					}
				}
				return result;
			}
		}

		public ZInt ItemsUnderSixteenOunces
		{
			get
			{
				ZInt result = ZInt.Zero;
				if (ExportAWBHeader.Consol != null)
				{
					foreach (ForwardingShipment currentShippment in ExportAWBHeader.Consol.Shipments)
					{
						foreach (PackLine currentPackLine in currentShippment.OuterPackLines)
						{
							if (Core.Constants.Weight.ConvertSafe(currentPackLine.JL_ActualWeight, currentPackLine.JL_ActualWeightUQ, "OZ") < 16m)
							{
								result++;
							}
						}
					}
				}
				return result;
			}
		}

		public ZInt NumberOfKnownShippers
		{
			get
			{
				ZInt result = ZInt.Zero;
				if (ExportAWBHeader.Consol != null)
				{
					foreach (ForwardingShipment currentShippment in ExportAWBHeader.Consol.Shipments)
					{
						if (IsKnownShipper(currentShippment.Consignor, (currentShippment.ConsignorDocumentaryAddress.HasRealAddress ? currentShippment.ConsignorDocumentaryAddress.Address : null)))
						{
							result++;
						}
					}
				}
				return result;
			}
		}

		public ZInt NumberOfUnKnownShippers
		{
			get
			{
				ZInt result = ZInt.Zero;
				if (ExportAWBHeader.Consol != null)
				{
					foreach (ForwardingShipment currentShippment in ExportAWBHeader.Consol.Shipments)
					{
						if (currentShippment.Consignor != null &&
							!IsKnownShipper(currentShippment.Consignor, (currentShippment.ConsignorDocumentaryAddress.HasRealAddress ? currentShippment.ConsignorDocumentaryAddress.Address : null)))
						{
							result++;
						}
					}
				}
				return result;
			}
		}

		bool IsKnownShipper(OrgHeader org, OrgAddress address)
		{
			bool orgIsKnown = org != null && org.CountryData != null && org.CountryData.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembershipEx.Codes.Yes;
			bool addressIsKnown = address != null && address.KnownShipperDetails.Count > 0 && address.KnownShipperDetails[0].OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembershipEx.Codes.Yes;

			return orgIsKnown || addressIsKnown;
		}

		public ZString InspectedShipmentText
		{
			get
			{
				ForwardingConsol consolToUse = null;
				ForwardingShipment shipmentToUse = null;

				if (ShipmentExportAWBHeader != null
					&& (!ShipmentExportAWBHeader.Shipment.IsDomestic() || IsFromToSpainToFromCanaryIslands(ShipmentExportAWBHeader.Shipment.Origin, ShipmentExportAWBHeader.Shipment.Destination))) // Sarah refused to check if Canaries are international for other use cases
				{
					shipmentToUse = ShipmentExportAWBHeader.Shipment;
				}
				else if (ConsolExportAWBHeader != null
					&& ConsolExportAWBHeader.Consol.Shipments.Count > 0
					&& (!ConsolExportAWBHeader.Consol.IsDomestic() || IsFromToSpainToFromCanaryIslands(ConsolExportAWBHeader.Consol.LoadPort, ConsolExportAWBHeader.Consol.DischargePort))) // Sarah refused to check if Canaries are international for other use cases
				{
					consolToUse = ConsolExportAWBHeader.Consol;
				}

				ZString result = string.Empty;

				if (shipmentToUse != null)
				{
					result = GetApprovedTextFromRegistry(shipmentToUse.AviationSecurity.IsApprovedForAviationSecurity);
				}
				else if (consolToUse != null)
				{
					result = GetApprovedTextFromRegistry(consolToUse.AreAllShipmentsApprovedForAviationSecurity);
				}

				return result;
			}
		}

		bool IsFromToSpainToFromCanaryIslands(RefUNLOCO origin, RefUNLOCO destination)
		{
			bool isWithinSpain = false;
			int toOrFromCanaryIslands = 0;

			if (origin != null && destination != null)
			{
				isWithinSpain = origin.RL_RN_NKCountryCode == "ES" && destination.RL_RN_NKCountryCode == "ES";
				toOrFromCanaryIslands = 0;

				if (origin.CountryStates != null && (origin.CountryStates.RW_Code == "TF" || origin.CountryStates.RW_Code == "GC"))
				{
					toOrFromCanaryIslands++;
				}
				if (destination.CountryStates != null && (destination.CountryStates.RW_Code == "TF" || destination.CountryStates.RW_Code == "GC"))
				{
					toOrFromCanaryIslands++;
				}
			}

			return isWithinSpain && toOrFromCanaryIslands == 1;
		}

		string GetApprovedTextFromRegistry(bool isApproved)
		{
			try
			{
				if (isGettingApprovedTextFromRegistry)
				{
					return string.Empty;
				}

				isGettingApprovedTextFromRegistry = true;

				var result = (string)(isApproved
					? FreightDataRegistry.Instance.AWBApprovedExporterText.Value
					: FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value);

				var macroReplacer = new MacroStringReplacer(new DataProviderList(this));
				return macroReplacer.ReplaceMacros(result);
			}
			finally
			{
				isGettingApprovedTextFromRegistry = false;
			}
		}

		bool isGettingApprovedTextFromRegistry;

		public ZString ConsolContainsDG
		{
			get
			{
				ZString result = "N";
				if (ExportAWBHeader.Consol != null)
				{
					foreach (ForwardingShipment currentShippment in ExportAWBHeader.Consol.Shipments)
					{
						foreach (PackLine currentPackLine in currentShippment.OuterPackLines)
						{
							foreach (UNDGDataItem dgItem in currentPackLine.UNDGs)
							{
								if (dgItem.Substance != null)
								{
									result = "Y";
									break;
								}
							}
						}
						if (result == "Y")
						{
							break;
						}
					}
				}
				return result;
			}
		}

		public ZString ServiceLevel
		{
			get
			{
				ZString result = ZString.Empty;
				if (ExportAWBHeader.Consol != null)
				{
					result = ExportAWBHeader.Consol.JK_AWBServiceLevel;
				}
				return result;
			}
		}

		public ZString ServiceLevelDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (ExportAWBHeader.Consol != null)
				{
					result = ExportAWBHeader.Consol.NeutralAirWaybillServiceLevelList.GetDescriptionFromCode(ExportAWBHeader.Consol.JK_AWBServiceLevel);
				}
				return result;
			}
		}

		public ZString ChargesAtDestination
		{
			get { return ZString.Empty; }
		}

		public ZString FAAIndirectAirCarrierNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates));
				}
				return result;
			}
		}

		public ZString IssuingCarrierIVACode
		{
			get
			{
				var iivRow = ExportAWBHeader.AWBAccountingInformations.Cast<ExportAWBAccountingInformation>().FirstOrDefault(info => info.IsItalianRegistrationCode && info.EA_InformationID == ExportAWBAccountingInformationLookups.IssuedByIVA);

				if (iivRow != null)
				{
					return iivRow.EA_Information;
				}

				return ZString.Empty;
			}
		}

		public ZString ShipperCodiceFiscaleOrIVA
		{
			get
			{
				var codiceFiscaleRow = ExportAWBHeader.AWBAccountingInformations.Cast<ExportAWBAccountingInformation>().FirstOrDefault(info => info.IsItalianRegistrationCode && info.EA_InformationID == ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA);

				if (codiceFiscaleRow != null)
				{
					return codiceFiscaleRow.EA_Information;
				}

				return ZString.Empty;
			}
		}

		public ZString IssuingCarrierNameAndAddress
		{
			get
			{
				ZString result = ExportAWBHeader.EH_IssuingAgentName + System.Environment.NewLine + ExportAWBHeader.EH_IssuingAgentAddress1 + System.Environment.NewLine + ExportAWBHeader.EH_IssuingAgentAddress2;
				return result.ToUpper();
			}
		}

		public ZBool WeightPPD
		{
			get { return ExportAWBHeader.EH_WeightPPD; }
		}

		public ZBool WeightCOL
		{
			get { return ExportAWBHeader.EH_WeightCOL; }
		}

		public ZBool OtherPPD
		{
			get { return ExportAWBHeader.EH_OtherPPD; }
		}

		public ZBool OtherCOL
		{
			get { return ExportAWBHeader.EH_OtherCOL; }
		}

		#region TotalLineTotals

		public ZString TotalLineTotals(ZBool asAgreed)
		{
			return (asAgreed || ExportAWBHeader.EH_TotalLineTotals == 0) ? ZString.Empty : FormatMoneyValue(ExportAWBHeader.EH_TotalLineTotals);
		}

		public ZString TotalLineTotals1
		{
			get
			{
				var isAsAgreed =
					ExportAWBHeader.AWBRateLines.Cast<ExportAWBRateLine>()
						.Any(line => (IsRateLineAsAgreed(line, ExportAWBHeader.EH_AsAgreed1st)));

				return TotalLineTotals(isAsAgreed);
			}
		}

		public ZString TotalLineTotals2
		{
			get
			{
				var isAsAgreed =
					ExportAWBHeader.AWBRateLines.Cast<ExportAWBRateLine>()
						.Any(line => (IsRateLineAsAgreed(line, ExportAWBHeader.EH_AsAgreed2nd)));

				return TotalLineTotals(isAsAgreed);
			}
		}

		#endregion

		public ZInt TotalNoOfPieces
		{
			get { return ExportAWBHeader.EH_TotalNoOfPieces; }
		}

		public ZString TotalGrossWeight
		{
			get { return ExportAWBHeader.EH_TotalGrossWeight.ToString(ExportAWBHeader.TotalGrossWeightDecimalPlaces); }
		}

		public ZString ReferenceNumber
		{
			get { return ExportAWBHeader.EH_ReferenceNumber; }
		}

		public ZString OptionalShippingInformation
		{
			get { return ExportAWBHeader.EH_OptionalShippingInformation; }
		}

		public ZString OptionalShippingInformation2
		{
			get { return ExportAWBHeader.EH_OptionalShippingInformation2; }
		}

		public ZString ECNCRNNumber
		{
			get { return ExportAWBHeader.EH_ECNCRNNumber.ToUpper(); }
		}

		#region TotalCOL

		public ZString TotalCOL(ZBool isAsAgreed)
		{
			if (ExportAWBHeader.EH_TotalCOL == 0)
			{
				return string.Empty;
			}

			return isAsAgreed
				? AsAgreedText
				: FormatMoneyValue(ExportAWBHeader.EH_TotalCOL);
		}

		public ZString TotalCOL1
		{
			get { return TotalCOL(IsCollectAsAgreed1st); }
		}

		public ZString TotalCOL2
		{
			get { return TotalCOL(IsCollectAsAgreed2nd); }
		}

		#endregion

		#region TotalPPD

		public ZString TotalPPD(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_TotalPPD == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_TotalPPD);
			}
		}

		public ZString TotalPPD1
		{
			get { return TotalPPD(IsPrepaidAsAgreed1st); }
		}

		public ZString TotalPPD2
		{
			get { return TotalPPD(IsPrepaidAsAgreed2nd); }
		}

		#endregion

		#region OtherChargesDueAgentCOL

		public ZString OtherChargesDueAgentCOL(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_OtherChargesDueAgentCOL == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_OtherChargesDueAgentCOL);
			}
		}

		public ZString OtherChargesDueAgentCOL1
		{
			get { return OtherChargesDueAgentCOL(IsCollectAsAgreed1st); }
		}

		public ZString OtherChargesDueAgentCOL2
		{
			get { return OtherChargesDueAgentCOL(IsCollectAsAgreed2nd); }
		}

		#endregion

		#region OtherChargesDueAgentPPD

		public ZString OtherChargesDueAgentPPD(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_OtherChargesDueAgentPPD == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_OtherChargesDueAgentPPD);
			}
		}

		public ZString OtherChargesDueAgentPPD1
		{
			get { return OtherChargesDueAgentPPD(IsPrepaidAsAgreed1st); }
		}

		public ZString OtherChargesDueAgentPPD2
		{
			get { return OtherChargesDueAgentPPD(IsPrepaidAsAgreed2nd); }
		}

		#endregion

		#region OtherChargesDueCarrierCOL

		public ZString OtherChargesDueCarrierCOL(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_OtherChargesDueCarrierCOL == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_OtherChargesDueCarrierCOL);
			}
		}

		public ZString OtherChargesDueCarrierCOL1
		{
			get { return OtherChargesDueCarrierCOL(IsCollectAsAgreed1st); }
		}

		public ZString OtherChargesDueCarrierCOL2
		{
			get { return OtherChargesDueCarrierCOL(IsCollectAsAgreed2nd); }
		}

		#endregion

		#region OtherChargesDueCarrierPPD

		public ZString OtherChargesDueCarrierPPD(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_OtherChargesDueCarrierPPD == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_OtherChargesDueCarrierPPD);
			}
		}

		public ZString OtherChargesDueCarrierPPD1
		{
			get { return OtherChargesDueCarrierPPD(IsPrepaidAsAgreed1st); }
		}

		public ZString OtherChargesDueCarrierPPD2
		{
			get { return OtherChargesDueCarrierPPD(IsPrepaidAsAgreed2nd); }
		}

		#endregion

		#region TotalWeightCOL

		public ZString TotalWeightCOL(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_TotalWeightCOL == 0)
			{
				return string.Empty;
			}

			return asAgreed
				? AsAgreedText
				: FormatMoneyValue(ExportAWBHeader.EH_TotalWeightCOL);
		}

		public ZString TotalWeightCOL1
		{
			get { return TotalWeightCOL(IsCollectAsAgreed1st); }
		}

		public ZString TotalWeightCOL2
		{
			get { return TotalWeightCOL(IsCollectAsAgreed2nd); }
		}

		#endregion

		#region TotalWeightPPD

		public ZString TotalWeightPPD(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_TotalWeightPPD == 0)
			{
				return string.Empty;
			}

			return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_TotalWeightPPD);
		}

		public ZString TotalWeightPPD1
		{
			get { return TotalWeightPPD(IsPrepaidAsAgreed1st); }
		}

		public ZString TotalWeightPPD2
		{
			get { return TotalWeightPPD(IsPrepaidAsAgreed2nd); }
		}

		#endregion

		#region As Agreed

		bool IsPrepaidAsAgreed1st
		{
			get { return ExportAWBHeader.EH_AsAgreed1st == Core.Constants.AWB.AsAgreedTypes.Codes.All; }
		}

		bool IsCollectAsAgreed1st
		{
			get
			{
				return ExportAWBHeader.EH_AsAgreed1st == Core.Constants.AWB.AsAgreedTypes.Codes.All
					|| ExportAWBHeader.EH_AsAgreed1st == Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			}
		}

		bool IsPrepaidAsAgreed2nd
		{
			get
			{
				return ExportAWBHeader.EH_AsAgreed2nd == Core.Constants.AWB.AsAgreedTypes.Codes.All
					|| ExportAWBHeader.EH_AsAgreed2nd == Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			}
		}

		bool IsCollectAsAgreed2nd
		{
			get
			{
				return ExportAWBHeader.EH_AsAgreed2nd == Core.Constants.AWB.AsAgreedTypes.Codes.All;
			}
		}

		static bool IsPaymentAsAgreed(ZString payment, ZString asAgreedString)
		{
			ZString asAgreedFromPayment;

			switch (payment)
			{
				case ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect:
				case ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect:
					asAgreedFromPayment = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
					break;
				case ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid:
				case ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid:
					asAgreedFromPayment = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
					break;
				case ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both:
				case ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both:
					asAgreedFromPayment = Core.Constants.AWB.AsAgreedTypes.Codes.All;
					break;
				default:
					asAgreedFromPayment = ZString.Empty;
					break;
			}

			return asAgreedString != Core.Constants.AWB.AsAgreedTypes.Codes.None
					&& (asAgreedString == Core.Constants.AWB.AsAgreedTypes.Codes.All
						|| asAgreedFromPayment == Core.Constants.AWB.AsAgreedTypes.Codes.All
						|| asAgreedFromPayment == asAgreedString);
		}

		bool IsRateLineAsAgreed(ExportAWBRateLine rateLine, ZString asAgreed)
		{
			if (asAgreed == Core.Constants.AWB.AsAgreedTypes.Codes.All)
			{
				return true;
			}

			if (asAgreed == Core.Constants.AWB.AsAgreedTypes.Codes.None)
			{
				return false;
			}

			var rateLinePayment = ZString.Empty;

			if (ExportAWBHeader.EH_WeightPPD || ExportAWBHeader.EH_WeightCOL)
			{
				rateLinePayment = ExportAWBHeader.EH_WeightPrepaidCollect;
			}

			if (ExportAWBHeader.EH_WeightPrepaidCollect == ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both)
			{
				rateLinePayment = rateLine.ER_RateClass;
			}

			return !rateLinePayment.IsEmpty && IsPaymentAsAgreed(rateLinePayment, asAgreed);
		}

		#endregion

		#region Agent

		public ZString AgentAccountNo
		{
			get { return ExportAWBHeader.EH_AgentAccountNo.ToUpper(); }
		}

		public ZString AgentIATACode
		{
			get { return ExportAWBHeader.EH_AgentIATACodeFormatted.ToUpper(); }
		}

		public ZString AgentName
		{
			get { return ExportAWBHeader.EH_AgentName.ToUpper(); }
		}

		public ZString AgentNameForLabel
		{
			get
			{
				return ShortenName(ExportAWBHeader.EH_AgentName, 85, new Font("Arial", 14, System.Drawing.FontStyle.Bold), false);
			}
		}

		ZString ShortenName(ZString nameToShorten, ZInt maxWidth, Font font, bool shouldTryTitleCase)
		{
			var widthCalculator = new TextSizeCalculator(font);
			var nameThatFits = nameToShorten.Trim();

			for (var i = 1; i < 6; i++)
			{
				if (widthCalculator.GetLengthInMillimeter(nameThatFits.ToString()) <= maxWidth)
				{
					break;
				}

				switch (i)
				{
					case 1:
					case 5:
						if (shouldTryTitleCase)
						{
							nameThatFits = nameThatFits.ToTitleCase();
						}
						break;
					case 2:
						nameThatFits = OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).RemoveNonLetterChars(nameThatFits.ToString(), true, false);
						break;
					case 3:
						nameThatFits = OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).RemoveExtraSpaces(nameThatFits.ToString());
						break;
					case 4:
						nameThatFits = OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).RemoveIgnoredOrganisationWords(nameThatFits.ToString());
						break;
				}
			}
			return nameThatFits;
		}

		public ZString AgentParticipantIdentifier
		{
			get { return ExportAWBHeader.EH_AgentParticipantIdentifier.ToUpper(); }
		}

		public ZString AgentPlace
		{
			get { return ExportAWBHeader.EH_AgentPlace.ToUpper(); }
		}

		#endregion

		#region Airline - Airport

		public ZString AirlinePrefix
		{
			get { return ExportAWBHeader.EH_AirlinePrefix.ToUpper(); }
		}

		public ZString AirportOfDepartureAndRequestRouteText
		{
			get { return ExportAWBHeader.EH_AirportOfDepartureAndRequestRouteText.ToUpper(); }
		}

		public ZString AirportOfDestinationCode
		{
			get { return ExportAWBHeader.EH_AirportOfDestinationCode.ToUpper(); }
		}

		public ZString AirportOfDestinationText
		{
			get { return ExportAWBHeader.EH_AirportOfDestinationText.ToUpper(); }
		}

		public ZString UltimateDestination
		{
			get
			{
				ZString result = string.Empty;

				ShipmentExportAWBHeader shipmentHeader = ExportAWBHeader as ShipmentExportAWBHeader;

				if (shipmentHeader != null)
				{
					if (shipmentHeader.Shipment.Declarations.Length > 0)
					{
						var declaration = ShipmentExportAWBHeader.Shipment.Declarations[0];
						RefUNLOCO loco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, declaration.JE_RL_NKFinalDestination));
						result = loco == null ? ZString.Empty : loco.Country.Description;
					}

					if (result.IsEmpty)
					{
						result = shipmentHeader.Shipment.Destination != null ? shipmentHeader.Shipment.Destination.Country.Description : ZString.Empty;
					}
				}
				else
				{
					ConsolExportAWBHeader consolHeader = ExportAWBHeader as ConsolExportAWBHeader;

					if (consolHeader != null)
					{
						result = consolHeader.Consol.DischargePort != null ? consolHeader.Consol.DischargePort.Country.Description : ZString.Empty;
					}
				}

				return result;
			}
		}

		#endregion

		#region Also Notify

		public ZBool IsNotifyOverriden
		{
			get { return ExportAWBHeader.EH_IsNotifyOverriden; }
		}

		public ZString AlsoNotifyAddress
		{
			get { return ExportAWBHeader.EH_AlsoNotifyAddress; }
		}

		public ZString AlsoNotifyContactCode => AlsoNotifyContactDetail.Length == 0 ? ZString.Empty : ExportAWBHeader.EH_AlsoNotifyContactCode;

		public ZString AlsoNotifyContactDetail
		{
			get { return ExportAWBHeader.EH_AlsoNotifyContactDetail; }
		}

		public ZString AlsoNotifyCountryCode
		{
			get { return ExportAWBHeader.EH_AlsoNotifyCountryCode; }
		}

		public ZString AlsoNotifyName
		{
			get { return ExportAWBHeader.EH_AlsoNotifyName; }
		}

		public ZString AlsoNotifyPlace
		{
			get { return ExportAWBHeader.EH_AlsoNotifyPlace; }
		}

		public ZString AlsoNotifyState
		{
			get { return ExportAWBHeader.EH_AlsoNotifyState; }
		}

		public ZString AlsoNotifyPostCode
		{
			get { return ExportAWBHeader.EH_AlsoNotifyPostCode; }
		}

		#endregion

		public ZString AWBAgentsSignature
		{
			get { return ExportAWBHeader.EH_AWBAgentsSignature.ToUpper() + " " + ExportAWBHeader.EH_AgentApprovedExporterNumber; }
		}

		public ZDateTime AWBIssueDate
		{
			get { return ExportAWBHeader.EH_AWBIssueDate; }
		}

		public ZString AWBIssuePlace
		{
			get { return ExportAWBHeader.EH_AWBIssuePlace.ToUpper(); }
		}

		public ZString AWBOriginCode
		{
			get { return ExportAWBHeader.EH_AWBOriginCode.ToUpper(); }
		}

		public ZString AWBSerialNo
		{
			get { return ExportAWBHeader.EH_AWBSerialNo; }
		}

		#region Booking Info

		public ZString Booking1stCarrier
		{
			get { return ExportAWBHeader.EH_Booking1stCarrier; }
		}

		public ZString Booking1stFlight
		{
			get { return ExportAWBHeader.EH_Booking1stFlight; }
		}

		public ZString Booking1stFlightDate
		{
			get { return ExportAWBHeader.EH_Booking1stFlightDate; }
		}

		public ZString Booking2ndCarrier
		{
			get { return ExportAWBHeader.EH_Booking2ndCarrier; }
		}

		public ZString Booking2ndFlight
		{
			get { return ExportAWBHeader.EH_Booking2ndFlight; }
		}

		public ZString Booking2ndFlightDate
		{
			get { return ExportAWBHeader.EH_Booking2ndFlightDate; }
		}

		#endregion

		#region By Position

		public ZString By1st
		{
			get { return ExportAWBHeader.EH_By1st; }
		}

		public ZString By2nd
		{
			get { return ExportAWBHeader.EH_By2nd; }
		}

		public ZString By3rd
		{
			get { return ExportAWBHeader.EH_By3rd; }
		}

		#endregion

		public ZString ChargesCode
		{
			get { return ExportAWBHeader.EH_ChargesCode; }
		}

		#region Consignee

		public ZString ConsigneeAccount
		{
			get { return ExportAWBHeader.EH_IsConsigneeOverriden ? ZString.Empty : ExportAWBHeader.EH_ConsigneeAccount.ToUpper(); }
		}

		public ZString ConsigneeContactCode => ConsigneeContactDetail.Length == 0 ? ZString.Empty : ExportAWBHeader.EH_ConsigneeContactCode;

		public ZString ConsigneeContactDetail
		{
			get { return ExportAWBHeader.EH_ConsigneeContactDetail; }
		}

		public ZString ConsigneeCountryCode
		{
			get { return ExportAWBHeader.EH_ConsigneeCountryCode; }
		}

		public ZString ConsigneePlace
		{
			get { return ExportAWBHeader.EH_ConsigneePlace; }
		}

		public ZString ConsigneeState
		{
			get { return ExportAWBHeader.EH_ConsigneeState; }
		}

		#endregion

		public ZString Currency
		{
			get { return ExportAWBHeader.EH_Currency; }
		}

		public ZString CustomsValue
		{
			get { return CustomsValueCore; }
		}

		public ZString RegistrationNumber
		{
			get { return ExportAWBHeader.RegistrationNumber; }
		}

		public ZString ExtraShipperData
		{
			get { return ExportAWBHeader.ExtraShipperData; }
		}

		protected virtual ZString CustomsValueCore
		{
			get
			{
				ZString result = string.Empty;
				if (ExportAWBHeader.EH_CustomsValue == 0M)
				{
					result = "NCV";
				}
				else
				{
					if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House)
					{
						result = FormatMoneyValue(ExportAWBHeader.EH_CustomsValue, ExportAWBHeader.EH_HouseCustomsValueCurrency) +
								" " + ExportAWBHeader.EH_HouseCustomsValueCurrency;
					}
					else
					{
						result = FormatMoneyValue(ExportAWBHeader.EH_CustomsValue);
					}
				}

				return result;
			}
		}

		public ZString DeclaredValue
		{
			get
			{
				ZString result = string.Empty;
				if (ExportAWBHeader.EH_DeclaredValue == 0M)
				{
					result = "NVD";
				}
				else
				{
					if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House)
					{
						result = FormatMoneyValue(ExportAWBHeader.EH_DeclaredValue, ExportAWBHeader.EH_HouseDeclaredValueCurrency) +
								" " + ExportAWBHeader.EH_HouseDeclaredValueCurrency;
					}
					else
					{
						result = FormatMoneyValue(ExportAWBHeader.EH_DeclaredValue);
					}
				}

				return result;
			}
		}

		public ZString HandlingInformation
		{
			get
			{
				ZString result = ExportAWBHeader.EH_HandlingInformation.ToUpper();

				if (!InspectedShipmentText.IsEmpty)
				{
					if (!result.IsEmpty)
					{
						result += " ";
					}

					result += InspectedShipmentText;
				}

				return result + " ";
			}
		}

		public ZString SCI
		{
			get { return ExportAWBHeader.EH_SpecialHandlingCode.ToUpper(); }
		}

		public ZString InsuranceValue
		{
			get
			{
				ZString result = string.Empty;
				if (ExportAWBHeader.EH_InsuranceValue == 0M)
				{
					result = "XXX";
				}
				else
				{
					if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House)
					{
						result = FormatMoneyValue(ExportAWBHeader.EH_InsuranceValue, ExportAWBHeader.EH_HouseInsuranceValueCurrency) +
								" " + ExportAWBHeader.EH_HouseInsuranceValueCurrency;
					}
					else
					{
						result = FormatMoneyValue(ExportAWBHeader.EH_InsuranceValue);
					}
				}

				return result;
			}
		}

		public ZString OtherPPDCOL
		{
			get { return ExportAWBHeader.EH_OtherPPDCOL; }
		}

		public ZGuid ParentID
		{
			get { return ExportAWBHeader.EH_ParentID; }
		}

		#region Shipper

		public ZString ShipperAccount
		{
			get { return ExportAWBHeader.EH_IsShipperOverriden ? ZString.Empty : ExportAWBHeader.EH_ShipperAccount.ToUpper(); }
		}

		public ZString ShipperContactCode => ShipperContactDetail.Length == 0 ? ZString.Empty : ExportAWBHeader.EH_ShipperContactCode;

		public ZString ShipperContactDetail
		{
			get { return ExportAWBHeader.EH_ShipperContactDetail; }
		}

		public ZString ShipperCountryCode
		{
			get { return ExportAWBHeader.EH_ShipperCountryCode; }
		}

		public ZString ShipperPlace
		{
			get { return ExportAWBHeader.EH_ShipperPlace; }
		}

		public ZString ShippersSignature
		{
			get { return ExportAWBHeader.EH_ShippersSignature.ToUpper(); }
		}

		public ZString ExtraShipperInfoLine1
		{
			get { return ExportAWBHeader.EH_ExtraShipperInfoLine1.ToUpper(); }
		}

		public ZString ExtraShipperInfoLine2
		{
			get { return ExportAWBHeader.EH_ExtraShipperInfoLine2.ToUpper(); }
		}

		public ZString ExtraCarrierInfoLine
		{
			get { return ExportAWBHeader.EH_ExtraCarrierInfoLine2.ToUpper(); }
		}

		public ZString ShipperState
		{
			get { return ExportAWBHeader.EH_ShipperState; }
		}

		#endregion

		public ZString Table
		{
			get { return ExportAWBHeader.EH_Table; }
		}

		#region TaxesCOL

		public ZString TaxesCOL(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_TaxesCOL == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_TaxesCOL);
			}
		}

		public ZString TaxesCOL1
		{
			get { return TaxesCOL(IsCollectAsAgreed1st); }
		}

		public ZString TaxesCOL2
		{
			get { return TaxesCOL(IsCollectAsAgreed2nd); }
		}

		#endregion

		#region TaxesPPD

		public ZString TaxesPPD(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_TaxesPPD == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_TaxesPPD);
			}
		}

		public ZString TaxesPPD1
		{
			get { return TaxesPPD(IsPrepaidAsAgreed1st); }
		}

		public ZString TaxesPPD2
		{
			get { return TaxesPPD(IsPrepaidAsAgreed2nd); }
		}

		#endregion

		#region ToPosition

		public ZString To1st
		{
			get { return ExportAWBHeader.EH_To1st; }
		}

		public ZString To2nd
		{
			get { return ExportAWBHeader.EH_To2nd; }
		}

		public ZString To3rd
		{
			get { return ExportAWBHeader.EH_To3rd; }
		}

		#endregion

		#region ValuationCOL

		public ZString ValuationCOL(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_ValuationCOL == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_ValuationCOL);
			}
		}

		public ZString ValuationCOL1
		{
			get { return ValuationCOL(IsCollectAsAgreed1st); }
		}

		public ZString ValuationCOL2
		{
			get { return ValuationCOL(IsCollectAsAgreed2nd); }
		}

		#endregion

		#region ValuationPPD

		public ZString ValuationPPD(ZBool asAgreed)
		{
			if (ExportAWBHeader.EH_ValuationPPD == 0)
			{
				return string.Empty;
			}
			else
			{
				return asAgreed ? AsAgreedText : FormatMoneyValue(ExportAWBHeader.EH_ValuationPPD);
			}
		}

		public ZString ValuationPPD1
		{
			get { return ValuationPPD(IsPrepaidAsAgreed1st); }
		}

		public ZString ValuationPPD2
		{
			get { return ValuationPPD(IsPrepaidAsAgreed2nd); }
		}

		#endregion

		public ZString WeightVPPDCOL
		{
			get { return ExportAWBHeader.EH_WeightVPPDCOL; }
		}

		public virtual ZString ConsolNumber
		{
			get { return ExportAWBHeader.EH_ConsolNumber; }
		}

		public ZString NetRate
		{
			get { return ExportAWBHeader.EH_NetRateCode; }
		}

		#region AWBLabel

		public ZString AirlineName
		{
			get { return ExportAWBHeader.EH_AirlineName.ToUpper(); }
		}

		public ZString AirlineShortName
		{
			get
			{
				if (!ExportAWBHeader.EH_AirlineShortName.IsEmpty)
				{
					return ExportAWBHeader.EH_AirlineShortName.ToUpper();
				}
				else
				{
					return ShortenName(AirlineName, 85, new Font("Arial", 16, System.Drawing.FontStyle.Bold), true);
				}
			}
		}

		public ZInt LabelTotalPacks
		{
			get { return ExportAWBHeader.MAWBLabelTotalPacks; }
		}

		public ZString LabelSize
		{
			get { return ExportAWBHeader.DocumentSize; }
		}

		#endregion

		#region OuterPacks

		public DocOuterPackCollection OuterPacks
		{
			get
			{
				var result = new DocOuterPackCollection(Factory);
				ZInt pieceCount = 0;

				if (ExportAWBHeader.Consol != null)
				{
					if (ShipmentExportAWBHeader != null)
					{
						ForwardingShipment shipment = ShipmentExportAWBHeader.Shipment;
						if (shipment != null)
						{
							TotalOuterPackCount = ExportAWBHeader.LabelTotalPacks != 0 ? ExportAWBHeader.LabelTotalPacks : shipment.JS_OuterPacks;
							AddShipmentOuterPacks(shipment, result, pieceCount, TotalOuterPackCount);
						}
					}
					else
					{
						fTotalOuterPackCount = (int)((decimal)ExportAWBHeader.Consol.JK_TotalShipmentQuantity);
						foreach (ForwardingShipment shipment in ExportAWBHeader.Consol.Shipments)
						{
							if (shipment.CoLoadMasterShipment == null
								|| !shipment.CoLoadMasterShipment.IsMasterShipmentRepresentingAllChildShipments
								|| !shipment.CoLoadMasterShipment.Consols.Contains(ExportAWBHeader.Consol.PK))
							{
								pieceCount = AddShipmentOuterPacks(shipment, result, pieceCount, shipment.JS_OuterPacks);
							}
						}
					}
				}

				return result;
			}
		}

		ZInt AddShipmentOuterPacks(ForwardingShipment shipment, DocOuterPackCollection outerPacks, ZInt pieceCount, ZInt totalPacks)
		{
			for (int i = 0; i < totalPacks; i++)
			{
				OuterPack outerPack = new OuterPack();
				outerPack.DocShipment = DocForwardingShipment.New(shipment, Factory);
				outerPack.Number = ++pieceCount;
				outerPack.ShipmentTotalPieceCount = totalPacks;

				if (pieceCount >= ExportAWBHeader.LabelStartRange && pieceCount <= ExportAWBHeader.LabelEndRange || ExportAWBHeader.LabelEndRange == 0)
				{
					outerPack.DocShipment.SequenceNumber = i + 1;

					outerPack.NumberInConsol = ExportAWBHeader.MAWBLabelStartRange + outerPacks.Count;
					outerPack.ConsolTotalPieceCount = ExportAWBHeader.MAWBLabelTotalPacks;

					string primaryBarcodeTextToEncode = AWBForLabel.AirlinePrefix + AWBForLabel.AWBSerialNo + "0" + outerPack.Number.ToString().PadLeft(4, '0');
					outerPack.EncodePrimaryBarcode(primaryBarcodeTextToEncode);
					outerPack.SetupOptionalInformation(this);

					outerPacks.Add(DocOuterPack.New(outerPack, Factory));
				}
			}

			return pieceCount;
		}

		public ZInt TotalOuterPackCount
		{
			get { return fTotalOuterPackCount; }
			set { fTotalOuterPackCount = value; }
		}
		ZInt fTotalOuterPackCount;

		public DocOuterPackCollection ShipmentOuterPacks
		{
			get
			{
				DocOuterPackCollection result = new DocOuterPackCollection(Factory);
				ZInt pieceCount = 0;
				if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House && ShipmentExportAWBHeader != null && DocShipment != null)
				{
					ForwardingShipment shipment = ShipmentExportAWBHeader.Shipment;
					if (shipment != null)
					{
						TotalOuterPackCount = shipment.JS_OuterPacks;
						for (int i = 0; i < shipment.JS_OuterPacks; i++)
						{
							OuterPack outerPack = new OuterPack();
							outerPack.DocShipment = DocShipment;
							outerPack.Number = ++pieceCount;

							if (pieceCount >= ExportAWBHeader.LabelStartRange && pieceCount <= ExportAWBHeader.LabelEndRange || ExportAWBHeader.LabelEndRange == 0)
							{
								string primaryBarcodeTextToEncode = ShipmentExportAWBHeader.EH_BillNumber + "0" + outerPack.Number.ToString().PadLeft(4, '0');
								outerPack.EncodePrimaryBarcode(primaryBarcodeTextToEncode);

								string secondaryBarcodeTextToEncode = "D" + AirportOfDestinationCode + "+" + "S" + shipment.TotalOuterPacks.ToString().PadLeft(4, '0');
								outerPack.EncodeSecondaryBarcode(secondaryBarcodeTextToEncode);

								outerPack.DocShipment.SequenceNumber = i + 1;
								result.Add(DocOuterPack.New(outerPack, Factory));
							}
						}
					}
				}

				return result;
			}
		}

		public DocOuterPackCollection ShipmentHazOuterPacks
		{
			get
			{
				DocOuterPackCollection result = new DocOuterPackCollection(Factory);
				ZInt pieceCount = 0;
				if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House && ShipmentExportAWBHeader != null && DocShipment != null)
				{
					foreach (DocPackLines line in DocShipment.OuterPackLineCollection)
					{
						if (line.IsHazardous)
						{
							for (int i = 0; i < line.PackageCount; i++)
							{
								OuterPack outerPack = new OuterPack();
								outerPack.DocShipment = DocShipment;
								outerPack.DocPackLines = line;
								outerPack.Number = ++pieceCount;
								outerPack.PackType = line.PackType;

								string primaryBarcodeTextToEncode = "H" + DocShipment.HouseBill;
								outerPack.EncodePrimaryBarcode(primaryBarcodeTextToEncode);

								result.Add(DocOuterPack.New(outerPack, Factory));
							}
						}
						TotalOuterPackCount = pieceCount;
					}
				}
				return result;
			}
		}
		#endregion

		bool IsHAWB
		{
			get { return ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House || ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.MasterHouse; }
		}

		public ZString INCO
		{
			get
			{
				ZString result = string.Empty;
				if (ShipmentExportAWBHeader != null && ShipmentExportAWBHeader.Shipment != null)
				{
					result = Core.Constants.IncoTerms.GetMappedOfficialIncoterm(ShipmentExportAWBHeader.Shipment.JS_INCO);
				}

				return result;
			}
		}

		public ZString UCR
		{
			get
			{
				ZString result = string.Empty;
				if (ShipmentExportAWBHeader != null && ShipmentExportAWBHeader.Shipment != null)
				{
					if (ShipmentExportAWBHeader.Shipment.Declarations.Length > 0)
					{
						var declaration = ShipmentExportAWBHeader.Shipment.Declarations[0];
						result = declaration.JE_AgentsReference;
					}
				}

				return result;
			}
		}

		#region AWB Label Optional Information

		public ZInt AWBLabelOptionalDesignNumber
		{
			get
			{
				ZInt result = 0;

				AWBLabelCustomisation awbLabel = FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.Value;
				if (awbLabel.CustomDesign == AWBLabelCustomDesignList.Codes.Default)
				{
					result = 0;
				}
				else if (awbLabel.CustomDesign == AWBLabelCustomDesignList.Codes.Design1)
				{
					result = 1;
				}
				else if (awbLabel.CustomDesign == AWBLabelCustomDesignList.Codes.Design2)
				{
					result = 2;
				}
				else if (awbLabel.CustomDesign == AWBLabelCustomDesignList.Codes.Design3)
				{
					result = 3;
				}

				return result;
			}
		}

		#endregion

		#region Supply Chain Security

		public ZString SecurityStatus
		{
			get { return SecurityStatusVisibility ? ExportAWBHeader.EH_SecurityStatusForNonBorrowedMAWBs : ZString.Empty; }
		}

		public ZString AviationSecurityApprovalNumber
		{
			get
			{
				var approval = GetCurrentOrgProxyAviationSecurityApproval();
				return approval == null ? ZString.Empty : approval.OV_EXApprovalNumber;
			}
		}

		public ZDate AviationSecurityApprovalExpiryDate
		{
			get
			{
				var approval = GetCurrentOrgProxyAviationSecurityApproval();
				return approval == null ? ZDate.Empty : approval.OV_EXApprovalExpiryDate;
			}
		}

		OrgCountryData GetCurrentOrgProxyAviationSecurityApproval()
		{
			if (SupplyChainSecurityConfiguration.IsAddressLevelScheme)
			{
				return GlbBranch.CurrentBranch.OrgProxy?.Addresses.Cast<OrgAddress>().FirstOrDefault(a => a.PK == ExportAWBHeader.EH_OA_ShipperAddress)?.KnownShipper
					?? GlbCompany.CurrentCompany.OrgProxy?.Addresses.Cast<OrgAddress>().FirstOrDefault(a => a.PK == ExportAWBHeader.EH_OA_ShipperAddress)?.KnownShipper
					?? GlbBranch.CurrentBranch.OrgProxy?.MainAddress?.KnownShipper
					?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress?.KnownShipper;
			}
			else
			{
				return GlbBranch.CurrentBranch.OrgProxy?.CountryData ?? GlbCompany.CurrentCompany.OrgProxy?.CountryData;
			}
		}

		SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New()); }
		}
		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion

		#region SecurityStatusVisibility

		public ZBool SecurityStatusVisibility
		{
			get { return ExportAWBHeader.SecurityStatusAWBVisibility; }
		}

		#endregion

		#endregion

		#region Terms & Conditions

		public ZBool TermsAndConditionsTextExists
		{
			get
			{
				return !TermsAndConditionsHeading.IsEmpty ||
					!TermsAndConditionsIntroduction.IsEmpty ||
					!TermsAndConditionsMiddleHeading.IsEmpty ||
					!TermsAndConditionsBody1.IsEmpty ||
					!TermsAndConditionsBody2.IsEmpty;
			}
		}

		public ZBool TermsAndConditionsIntroductionAndColumnsText
		{
			get { return !TermsAndConditionsIntroduction.IsEmpty && !TermsAndConditionsBody1.IsEmpty; }
		}

		public ZBool TermsAndConditionsColumnsTextOnly
		{
			get { return TermsAndConditionsIntroduction.IsEmpty && !TermsAndConditionsBody1.IsEmpty && !TermsAndConditionsBody2.IsEmpty; }
		}

		public ZString TermsAndConditionsHeading
		{
			get { return IsHAWB ? FreightDataRegistry.Instance.HAWBTermsAndConditionsHeading.Value : FreightDataRegistry.Instance.MAWBTermsAndConditionsHeading.Value; }
		}

		public ZString TermsAndConditionsIntroduction
		{
			get { return IsHAWB ? FreightDataRegistry.Instance.HAWBTermsAndConditionsIntroduction.Value : FreightDataRegistry.Instance.MAWBTermsAndConditionsIntroduction.Value; }
		}

		public ZString TermsAndConditionsMiddleHeading
		{
			get { return IsHAWB ? FreightDataRegistry.Instance.HAWBTermsAndConditionsMiddleHeading.Value : FreightDataRegistry.Instance.MAWBTermsAndConditionsMiddleHeading.Value; }
		}

		public ZString TermsAndConditionsBody1
		{
			get { return IsHAWB ? FreightDataRegistry.Instance.HAWBTermsAndConditionsBody1.Value : FreightDataRegistry.Instance.MAWBTermsAndConditionsBody1.Value; }
		}

		public ZString TermsAndConditionsBody2
		{
			get { return IsHAWB ? FreightDataRegistry.Instance.HAWBTermsAndConditionsBody2.Value : FreightDataRegistry.Instance.MAWBTermsAndConditionsBody2.Value; }
		}

		#endregion

		#region Images

		public Image BrandImage
		{
			get
			{
				if (fBrandImage == null || fBrandImage.IsDisposed())
				{
					if (IsHAWB)
					{
						return null;
					}

					var stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.AWBBrandingMaster_600b.png");
					fBrandImage = new Bitmap(stream);
				}

				return fBrandImage;
			}
		}
		Image fBrandImage;

		public Image HAWBImage
		{
			get
			{
				if (fHAWBImage == null || fHAWBImage.IsDisposed())
				{
					fHAWBImage = null;

					if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House && ShipmentExportAWBHeader != null && DocShipment != null)
					{
						DocOrganisation brandingAgent = DocShipment.Consol != null ? DocShipment.Consol.ReceivingForwarder : null;
						DocOrganisation brandingClient = DocShipment.Consignor;

						fHAWBImage = GetHouseBillBrandImage(brandingAgent, brandingClient, DocumentsDataRegistry.Instance.HAWBAgentBrandingImage);

						if (fHAWBImage == null)
						{
							fHAWBImage = Env.Registry.Freight.AirWaybill.HAWBLogo;
						}
					}
				}

				return fHAWBImage;
			}
		}
		Image fHAWBImage;

		public Image HAWBLogo
		{
			get { return HAWBImage; }
		}

		public Image IssuedByImage
		{
			get
			{
				if (fIssuedByImage == null || fIssuedByImage.IsDisposed())
				{
					string resourceName = string.Empty;
					if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House)
					{
						if (HAWBImage == null)
						{
							resourceName += LanguageOfTitle + "HouseTitle";
						}
					}
					else if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.MasterHouse)
					{
						resourceName += LanguageOfTitle + "MasterHouseTitle";
					}
					else
					{
						resourceName += LanguageOfTitle + "MasterTitle";
					}

					if (!string.IsNullOrEmpty(resourceName))
					{
						var stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.AWB" + resourceName + ".png");
						if (stream != null)
						{
							fIssuedByImage = new Bitmap(stream);
						}
					}
				}

				return fIssuedByImage;
			}
		}
		Image fIssuedByImage;

		public ZString LanguageOfTitle
		{
			get { return GetTemplateConstantValue(DocumentEngineIntegration.Constants.TemplateDefined.LanguageOfTitle, ZString.Empty); }
		}

		#endregion

		#region TSASecurityStatementCountriesText

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public ZString TSASecurityStatementCountriesText
		{
			get
			{
				var registryValue = FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.Value;
				var countries = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.PK, registryValue));

				var countryNames = countries.WhereNotNull().Select(c => c.RN_Desc).OrderBy(c => c);

				return string.Join(", ", countryNames);
			}
		}

		#endregion

		#region Signature

		public Image SignatureImage
		{
			get
			{
				if (signatureImage == null || signatureImage.IsDisposed())
				{
					signatureImage = null;

					if (ShouldPrintUserSignature)
					{
						var currentUser = GlbStaff.CurrentUser;
						signatureImage = currentUser.SignatureImage;

						if (signatureImage != null)
						{
							var usageDetailsCollector = Factory?.ServiceContainer.GetService<DocumentUsageDetailsCollector>();
							usageDetailsCollector?.GetIsUserSignatureUsed(true);
						}
					}
				}

				return signatureImage;
			}
		}

		Image signatureImage;

		public ZBool ShouldPrintUserSignature
		{
			get
			{
				return IsHAWB
					? FreightDataRegistry.Instance.PrintSignatureForHAWBDocuments.Value
					: FreightDataRegistry.Instance.PrintSignatureForMAWBDocuments.Value;
			}
		}

		#endregion

		#region Money Formatting

		ZString FormatMoneyValue(ZDecimal value, ZString currencyNK)
		{
			int decimals = 2;
			if (currencyNK.IsEmpty)
			{
				return FormatMoneyValue(value);
			}

			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyNK);
			if (currency != null)
			{
				decimals = currency.Decimals;
			}

			return value.ToString(decimals);
		}

		ZString FormatMoneyValue(ZDecimal value)
		{
			return value.ToString(DefaultCurrencyMoneyDecimals);
		}

		ZString FormatMoneyValue(ZDecimal value, int padLeftToThisMAnyChars)
		{
			return value.ToString(DefaultCurrencyMoneyDecimals).PadLeft(padLeftToThisMAnyChars, ' ');
		}

		int DefaultCurrencyMoneyDecimals
		{
			get
			{
				if (fDefaultCurrencyMoneyDecimals == -1)
				{
					fDefaultCurrencyMoneyDecimals = 2;
					if (!ExportAWBHeader.EH_Currency.IsEmpty)
					{
						RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ExportAWBHeader.EH_Currency);
						if (currency != null)
						{
							fDefaultCurrencyMoneyDecimals = currency.Decimals;
						}
					}
				}

				return fDefaultCurrencyMoneyDecimals;
			}
		}
		int fDefaultCurrencyMoneyDecimals = -1;

		#endregion

		#region IDocManagerBarcode Members

		ZString BillNumberBarcode
		{
			get
			{
				TextBarcode billNumberBarcode = new TextBarcode(BillNumber);
				return billNumberBarcode.TextAs128sFontString;
			}
		}

		protected override TextBarcode DocManagerBarcode
		{
			get
			{
				if (fDocManagerBarcode == null)
				{
					BarcodeGenerator generator = new BarcodeGenerator();
					fDocManagerBarcode = generator.CreateShipmentBarcode(GlbCompany.CurrentCompany.GC_Code, DocTypeCode, AWBOriginCode, AirportOfDestinationCode, BillNumber);
				}
				return fDocManagerBarcode;
			}
		}
		TextBarcode fDocManagerBarcode;

		public override ZString BarcodeTextForFont
		{
			get
			{
				if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House)
				{
					return DocManagerBarcode.TextAs128sFontString;
				}
				else
				{
					return BillNumberBarcode;
				}
			}
		}

		public override ZString BarcodeText
		{
			get
			{
				if (ExportAWBHeader.AWBType == ExportAWBHeader.TypeOfAWB.House)
				{
					return DocManagerBarcode.TextToEncode;
				}
				else
				{
					return BillNumber;
				}
			}
		}

		#endregion

		#region IDocTypeCode Members

		ZString IDocTypeCode.DocTypeCode
		{
			get { return DocTypeCode; }
			set { DocTypeCode = value; }
		}
		ZString DocTypeCode;

		#endregion

		#region ShouldUseBOLClauseITAR

		public ZBool ShouldUseBOLClauseITAR
		{
			get
			{
				return ZDateTime.Now < DocConstants.USDestinationControlStatement.EffectiveDate;
			}
		}

		#endregion
	}
}
