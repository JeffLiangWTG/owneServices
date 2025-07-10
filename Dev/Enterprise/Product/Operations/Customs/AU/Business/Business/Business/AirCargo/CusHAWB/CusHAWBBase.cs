using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Organizations.PatternMatching;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using IAddress = Enterprise.Customs.Business.IAddress;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CusHAWB.Schema.CS_MessageReference), DescriptionProperty(CusHAWB.Schema.CS_HAWB), SystemDefinedValues]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.AUCusHAWB)]
	public abstract class CusHAWBBase : Customs.Business.CusHAWB,
		Integration.Customs.AU.ICusHAWBBase,
		ICusUnderbondParent,
		Customs.Business.IStatusNeedsRecalculationProvider,
		IDocumentSupportable,
		IOutturnableLine,
		ICusHAWBBase,
		ICusUnderbondDependentCollectionParent,
		ICurrencyConverterDataProvider,
		IUniversalXMLNoteParent,
		ITransitWarehouseSyncEventParent
	{
		protected CusHAWBBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Calculator = new CusHAWBStatusCalculator(this);
			MessageStatusCalculator = new CusHAWBMessageStatusCalculator(this);
			phoneNumberPropertyHelperOfConsigneeThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbersForConsignee));
			phoneNumberPropertyHelperOfConsignorThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbersForConsignor));
		}

		ZString DefaultCountryCodeForPhoneNumbersForConsignee => string.IsNullOrEmpty(Consignee?.CountryCode) ? ZString.Empty : Consignee.CountryCode;

		ZString DefaultCountryCodeForPhoneNumbersForConsignor => string.IsNullOrEmpty(Consignor?.CountryCode) ? ZString.Empty : Consignor.CountryCode;

		PhoneNumberPropertyHelper PhoneNumberPropertyHelperOfConsignee => phoneNumberPropertyHelperOfConsigneeThunk.Value;

		PhoneNumberPropertyHelper PhoneNumberPropertyHelperOfConsignor => phoneNumberPropertyHelperOfConsignorThunk.Value;

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperOfConsigneeThunk;

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperOfConsignorThunk;

		public static new readonly TypeDecider TypeDecider = new CusHAWBBaseTypeDecider();

		public readonly CusHAWBStatusCalculator Calculator;
		public readonly CusHAWBMessageStatusCalculator MessageStatusCalculator;

		public static CusHAWBBase Load(BusinessObjectFactory factory, ZString senderReference)
		{
			if (senderReference.IndexOf('/') != -1)
			{
				senderReference = senderReference.Split('/')[0];
			}
			return CusHAWBBase.LoadFromQuery(new ZQuery(CusHAWBSchema.CS_MessageReference, senderReference), factory);
		}

		public static CusHAWBBase LoadFromQuery(ZQuery query, BusinessObjectFactory factory)
		{
			var hawbs = factory.Load<Customs.Business.CusHAWB>(query);
			return hawbs.OfType<CusHAWBBase>().FirstOrDefault(hawb =>
				{
					var mawb = hawb.MAWB;
					if (mawb != null)
					{
						return CusMAWBBase.Loader.CMRApplicationCodes.Contains(mawb.CM_ApplicationCode);
					}

					return CMRApplicationCodes.Contains(hawb.CS_ApplicationCode);
				});
		}

		/// <summary>
		/// This won't find detached CusHAWBs.
		/// </summary>
		public static CusHAWBBase[] LoadAllFromQuery(ZDBOnlyQuery query, BusinessObjectFactory factory, bool loadRecentOnly = false)
		{
			ZDBOnlyQuery result = query;

			ZDBOnlySubQuery cusMAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWBBase), CusHAWBSchema.CS_CM);
			cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, CusMAWBBase.Loader.CMRApplicationCodes);
			if (loadRecentOnly)
			{
				var dateQuery = new ZQuery(CusMAWBSchema.CM_ArrivalDate, ZDateTime.Empty);
				dateQuery.AddToFilter(JoinCondition.Or, CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
				cusMAWBQuery.AddToFilter(dateQuery);
			}
			result.AddSubQuery(cusMAWBQuery, JoinCondition.And);

			return factory.Load<CusHAWBBase>(result);
		}

		static ZString[] CMRApplicationCodes
		{
			get
			{
				return new ZString[] { ZString.Empty, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages };
			}
		}

		#region New Properties

		#region Schema

		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new abstract class Schema : Customs.Business.CusHAWB.Schema
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string CS_CustomsStatusCMR = "CS_CustomsStatusCMR";
			public const string CS_PaymentTypeCaption = "CS_PaymentTypeCaption";
			public const string CS_FreightPrepaidCollectForBinding = "CS_FreightPrepaidCollectForBinding";
			public const string CS_ShipmentTypeForBinding = "CS_ShipmentTypeForBinding";
			public const string CS_FlightNo = "CS_FlightNo";
			public const string CS_ArrivalDate = "CS_ArrivalDate";
			public const string CS_fPartShipConsignmentReference = "CS_fPartShipConsignmentReference";
			public const int CS_fPartShipConsignmentReferenceMaxLength = 35;
			public const int NewCS_GoodsDescriptionMaxLength = 2560;
		}

		#endregion

		#region Consignment Reference

		[MaxLength(Schema.CS_fPartShipConsignmentReferenceMaxLength)]
		public ZString CS_fPartShipConsignmentReference
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.CS_fPartShipConsignmentReference); }
			set
			{
				CheckMaximumLength(CS_fPartShipConsignmentReferenceInfo, value);
				this.SetSystemDefinedValue(Schema.CS_fPartShipConsignmentReference, value);
				CS_fPartShipConsignmentReferenceInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCS_fPartShipConsignmentReference();
				}
			}
		}

		public ZPropertyInfo CS_fPartShipConsignmentReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.CS_fPartShipConsignmentReference); }
		}

		public bool CS_fPartShipConsignmentReference_ReadOnly
		{
			get { return !AUCustomsDataRegistry.Instance.EnableDebugHooksForAU.Value; }
		}

		public void SetConsignmentReferenceIfNecessary()
		{
			if (MAWB != null && MAWB.IsAltPartShipModelActive && MAWB.CM_fUseAltPartShipModel
				&& CMRStatusHelper.NoMessageIsCurrent(CMRMessageStatus.Code))
			{
				if (!IsInDatabase)
				{
					PopulateNumberPropertyIfRequired(CS_fPartShipConsignmentReferenceInfo, GetNewConsignmentReference);
				}
				else if (CS_fPartShipConsignmentReference.IsEmpty)
				{
					CS_fPartShipConsignmentReference = GetNewConsignmentReference(Factory);
				}

#if DEBUG
				if (ForceExceptionAfterConRefAllocated)
				{
					throw new NotSupportedException("Crash Save");
				}
#endif
			}
		}

#if DEBUG
		public bool ForceExceptionAfterConRefAllocated;
#endif

		ZString GetNewConsignmentReference(BusinessObjectFactory factory)
		{
			var reference = ExistingConRefFor(CS_HAWB, Factory).Left(CS_fPartShipConsignmentReferenceInfo.MaxLength);
			if (reference.IsEmpty)
			{
				reference = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode + ZDate.Today.ToString("yyyyMMdd") + Env.NumberFountains.AUPartShipConRef.GetNextFormatted(factory);
			}
			return reference;
		}

		public static ZString ExistingConRefFor(ZString houseBillNum, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(GenAddOnColumn));
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, CusHAWB.Schema.CS_fPartShipConsignmentReference);
			query.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.NotEqual, ZString.Empty);
			var cusHAWBSubQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.PK);
			cusHAWBSubQuery.AddToFilter(CusHAWBSchema.CS_HAWB, houseBillNum);
			query.AddSubQuery(GenAddOnColumnSchema.XA_ParentID, cusHAWBSubQuery, JoinCondition.And);
			foreach (GenAddOnColumn conRef in factory.Load<GenAddOnColumn>(query))
			{
				if (ZDateTime.TryParseExact(conRef.XA_Data.SubstringSafe(3, 8), out var conRefDate, "yyyyMMdd") && conRefDate > ZDateTime.Now.AddMonths(-2))
				{
					return conRef.XA_Data;
				}
			}
			return ZString.Empty;
		}

		#endregion

		public bool IsDirect
		{
			get
			{
				return MAWB != null && MAWB.Consol != null && MAWB.Consol.JK_AgentType == Core.Constants.AgentType.Direct;
			}
		}

		public override ZString CS_MsgStatus
		{
			get { return base.CS_MsgStatus; }
			set
			{
				bool hasChanges = CS_MsgStatus != value;
				base.CS_MsgStatus = value;
				if (hasChanges && MAWB != null)
				{
					MAWB.RefreshShouldStopKeyFieldsChangeCalculation();
				}
			}
		}

		public ZDateTime CS_ArrivalDate
		{
			get { return MAWB != null ? MAWB.CM_ArrivalDate : ZDateTime.Empty; }
		}

		public override ZString CS_ResponsiblePartyID
		{
			get { return base.CS_ResponsiblePartyID; }
			set
			{
				ZString originalValue = base.CS_ResponsiblePartyID;
				base.CS_ResponsiblePartyID = value;
				if (originalValue != value && MAWB != null)
				{
					MAWB.MarkAsNeedingValidation();
				}
			}
		}

		public void PopulateResponsiblePartyFromConsigneeABNorCCID()
		{
			if (Consignee != null)
			{
				if (!Consignee.PrimaryRegistrationNumber.Number.IsEmpty)
				{
					CS_ResponsiblePartyID = Consignee.PrimaryRegistrationNumber.Number.Left(CS_ResponsiblePartyIDInfo.MaxLength);
				}
				else
				{
					CS_ResponsiblePartyID = Consignee.GetCustomsClientID(ConsigneeAddress).Left(CS_ResponsiblePartyIDInfo.MaxLength);
				}
			}
		}

		public ZPropertyInfo CS_ArrivalDateInfo
		{
			get { return GetZPropertyInfo(CusHAWB.Schema.CS_ArrivalDate); }
		}

		public bool IsImport
		{
			get { return !CS_RL_NKDestination.IsEmpty && CS_RL_NKDestination.SubstringSafe(0, 2) == GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2); }
		}

		public bool IsExport
		{
			get { return !CS_RL_NKOrigin.IsEmpty && CS_RL_NKOrigin.SubstringSafe(0, 2) == GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2); }
		}

		public ZString CS_FlightNo
		{
			get { return MAWB != null ? MAWB.CM_FlightNo : ZString.Empty; }
		}

		public ZPropertyInfo CS_FlightNoInfo
		{
			get { return GetZPropertyInfo(CusHAWB.Schema.CS_FlightNo); }
		}

		public string ConsigneeAddressAsASingleLine
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(GetStringWithPaddingSpace(CS_ConsigneeStreet));
				builder.Append(GetStringWithPaddingSpace(CS_ConsigneeStreet2));
				builder.Append(GetStringWithPaddingSpace(CS_ConsigneeCity));
				builder.Append(GetStringWithPaddingSpace(CS_ConsigneeState));
				builder.Append(CS_ConsigneePostcode);

				return builder.ToString();
			}
		}

		string GetStringWithPaddingSpace(string inputString)
		{
			return !string.IsNullOrEmpty(inputString) ? inputString + " " : inputString;
		}

		public ZString AggregatedCoLoadMaster
		{
			get { return CS_MasterHouseBill.IsEmpty && MAWB != null ? MAWB.CM_MasterHouseBill : CS_MasterHouseBill; }
		}

		[MaxLength(50)]
		public virtual ZString WarehouseLocationCaption
		{
			get { return "Warehouse Location:"; }
		}

		public ZPropertyInfo WarehouseLocationCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseLocationCaption)); }
		}

		[MaxLength(50)]
		public virtual ZString ChargableWeightCaption
		{
			get
			{
				return Res.GetString("0C89235F-8C5A-4B61-B42F-27A19D8641B7", "Chargeable Weight:");
			}
		}

		public ZPropertyInfo ChargableWeightCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(ChargableWeightCaption)); }
		}

		[MaxLength(20)]
		public virtual ZString CS_PaymentTypeCaption => "Method of Payment:";

		public ZPropertyInfo CS_PaymentTypeCaptionInfo
		{
			get { return GetZPropertyInfo(CusHAWBBase.Schema.CS_PaymentTypeCaption); }
		}

		public ZString CS_FreightPrepaidCollectForBinding
		{
			get { return (ZString)InnerPrepaidCollectInfo.Value; }
			set { InnerPrepaidCollectInfo.Value = value; }
		}

		public ZPropertyInfo CS_FreightPrepaidCollectForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(CusHAWBBase.Schema.CS_FreightPrepaidCollectForBinding, x => InnerPrepaidCollectInfo); }
		}

		public ZString CS_ShipmentTypeForBinding
		{
			get { return (ZString)InnerShipmentTypeInfo.Value; }
			set { InnerShipmentTypeInfo.Value = value; }
		}

		public ZPropertyInfo CS_ShipmentTypeForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(CusHAWBBase.Schema.CS_ShipmentTypeForBinding, x => InnerShipmentTypeInfo); }
		}

		protected virtual ZPropertyInfo InnerPrepaidCollectInfo
		{
			get { return CS_FreightPrepaidCollectInfo; }
		}

		protected virtual ZPropertyInfo InnerShipmentTypeInfo
		{
			get { return CS_ShipmentTypeInfo; }
		}

		public virtual ZString ShortDescription
		{
			get { return CS_HAWB.IsEmpty ? ZString.Empty : new ZString("HAWB: " + CS_HAWB); }
		}

		#region Message Status

		public MessageCusStatus CMRMessageStatus
		{
			get
			{
				if (fCMRMessageStatus == null)
				{
					fCMRMessageStatus = new MessageCusStatus(CS_MsgStatusInfo, MessageStatusCalculator);
				}
				return fCMRMessageStatus;
			}
		}
		MessageCusStatus fCMRMessageStatus;

		public ZString UnderbondStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (AllUnderbonds.Count > 1)
				{
					result = "More than one underbond";
				}
				else if (AllUnderbonds.Count == 1)
				{
					result = AllUnderbonds[0].UnderbondStatus.Description;
				}
				return result;
			}
		}

		public ZPropertyInfo UnderbondStatusInfo
		{
			get { return GetZPropertyInfo(nameof(UnderbondStatus)); }
		}

		#endregion

		#region Cargo Status

		public CargoCusStatus CMRCargoStatus
		{
			get
			{
				if (fCMRCargoStatus == null)
				{
					fCMRCargoStatus = new CargoCusStatus(CS_CustomsStatusInfo, Calculator);
				}
				return fCMRCargoStatus;
			}
		}
		CargoCusStatus fCMRCargoStatus;

		#endregion

		public virtual object ParentForCargoReportingEvents
		{
			get { return null; }
		}

		public ZString UnderbondHumanReadableName
		{
			get { return UnderbondHumanReadableNameCore; }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return CMRCargoStatus.Description; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return CS_PiecesManifested; }
		}

		public bool IsDocuments
		{
			get { return CS_ShipmentType == "DOC"; }
			set { CS_ShipmentType = value ? "DOC" : "STD"; }
		}

		public bool CanDeleteFromICusHAWBCollection
		{
			get { return true; }
		}

		#endregion

		[MaxLength(Schema.NewCS_GoodsDescriptionMaxLength)]
		public override ZString CS_GoodsDescription
		{
			get { return base.CS_GoodsDescription; }
			set { base.CS_GoodsDescription = value; }
		}

		#region Business Objects Overrides

		public override void OnLoaded()
		{
			if (!CMRApplicationCodes.Contains(CS_ApplicationCode))
			{
				ErrorReporter.ReportOnce("Attempting to load a non-AU CusHAWB as a AU CusHAWB",
					string.Format(CultureInfo.InvariantCulture, "Attempting to load a non-AU CusHAWB as a AU CusHAWB, CS_ApplicationCode: {0}, CM_ApplicationCode: {1}",
					CS_ApplicationCode, base.MAWB?.CM_ApplicationCode ?? "None")); // logging error message
			}

			base.OnLoaded();
			ReadOnly = CS_IsResponsePending;
		}

		public sealed override void Delete()
		{
			DeleteCore();
			if (fConsignorSubscriptionManager != null)
			{
				fConsignorSubscriptionManager.Enabled = false;
			}
			if (fConsigneeSubscriptionManager != null)
			{
				fConsigneeSubscriptionManager.Enabled = false;
			}
			base.Delete();
		}

		protected virtual void DeleteCore()
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();

			CheckHawbIsSingular();
			AddAHoldPrealertLogIfNecessary();
			SetConsignmentReferenceIfNecessary();

			if (CS_CM.IsValid)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CS_CM), IsInDatabase ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Default);
			}

			if (CS_MsgStatus.IsEmpty)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CS_MsgStatus), IsInDatabase ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Default);
			}
		}

		void CheckHawbIsSingular()
		{
			if (!IsInDatabase && CS_JS.IsValid && (MAWB?.Consol?.IsInDatabase ?? false))
			{
				var consolPK = MAWB.CM_JK;
				var consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
				consolQuery.AddToFilter(JobConsolSchema.PK, consolPK);
				consolQuery.TableHints |= TableHints.UPDLOCK;
				_ = Factory.Load<ForwardingConsol>(consolQuery);

				// Check for an existing CMR HAWB on this Shipment and Consol
				var mawbQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
				mawbQuery.AddToFilter(CusMAWBSchema.CM_JK, consolPK);
				mawbQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
				var hawbQuery = new ZDBOnlyQuery(typeof(CusHAWBBase));
				hawbQuery.AddToFilter(CusHAWBSchema.CS_JS, CS_JS);
				hawbQuery.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, PK);
				hawbQuery.AddSubQuery(mawbQuery, JoinCondition.And);

				if (Factory.ExistsInDatabase(CusHAWB.Schema.TableName, hawbQuery))
				{
					var otherHawb = Factory.Load<CusHAWBBase>(hawbQuery).FirstOrDefault();
					if (otherHawb != null)
					{
						var message = ZString.Format("A House Bill for this Shipment has already been created (On saving).\r\nThis Hawb {0}\r\nOther Hawb {1}", GetDetailsForHouseBill(this), GetDetailsForHouseBill(otherHawb));  // Error Reporting
						throw new ZArchitecture.Environment.OdysseyException(message);
					}
				}
			}
		}

		string GetDetailsForHouseBill(CusHAWBBase hawb)
		{
			var messageBuilder = new ZStringBuilder();
			messageBuilder.Append($"PK: {hawb.PK}");
			messageBuilder.Append($"HAWB: '{hawb.CS_HAWB}'");
			messageBuilder.Append($"MAWB: '{hawb.MAWB?.CM_MAWB}' ({hawb.CS_CM})");
			messageBuilder.Append($"Shipment: '{hawb.Shipment?.JobNumber}' ({hawb.CS_JS})");
			messageBuilder.Append($"Created: '{hawb.CS_SystemCreateTimeUtc.ToString("u")}' by '{hawb.CS_SystemCreateUser}'");

			return messageBuilder.ToStringWithNewLineBetweenAppends();
		}

		protected virtual void OnSavingConsignorCheck()
		{
			if (Consignor != null)
			{
				if (!CS_ConsignorCity.IsEmpty)
				{
					CS_ConsignorCity = ZString.Empty;
				}

				if (!CS_ConsignorName.IsEmpty)
				{
					CS_ConsignorName = ZString.Empty;
				}

				if (!CS_ConsignorPhone.IsEmpty)
				{
					CS_ConsignorPhone = ZString.Empty;
				}

				if (!CS_ConsignorPostcode.IsEmpty)
				{
					CS_ConsignorPostcode = ZString.Empty;
				}

				if (!CS_ConsignorState.IsEmpty)
				{
					CS_ConsignorState = ZString.Empty;
				}

				if (!CS_ConsignorStreet.IsEmpty)
				{
					CS_ConsignorStreet = ZString.Empty;
				}

				if (!CS_ConsignorStreet2.IsEmpty)
				{
					CS_ConsignorStreet2 = ZString.Empty;
				}

				if (!CS_RN_NKConsignorCountry.IsEmpty)
				{
					CS_RN_NKConsignorCountry = ZString.Empty;
				}
			}
		}

		protected virtual void OnSavingConsigneeCheck()
		{
			if (Consignee != null)
			{
				if (!CS_ConsigneeCity.IsEmpty)
				{
					CS_ConsigneeCity = ZString.Empty;
				}

				if (!CS_ConsigneeName.IsEmpty)
				{
					CS_ConsigneeName = ZString.Empty;
				}

				if (!CS_ConsigneePhone.IsEmpty)
				{
					CS_ConsigneePhone = ZString.Empty;
				}

				if (!CS_ConsigneePostcode.IsEmpty)
				{
					CS_ConsigneePostcode = ZString.Empty;
				}

				if (!CS_ConsigneeState.IsEmpty)
				{
					CS_ConsigneeState = ZString.Empty;
				}

				if (!CS_ConsigneeStreet.IsEmpty)
				{
					CS_ConsigneeStreet = ZString.Empty;
				}

				if (!CS_ConsigneeStreet2.IsEmpty)
				{
					CS_ConsigneeStreet2 = ZString.Empty;
				}

				if (!CS_RN_NKConsigneeCountry.IsEmpty)
				{
					CS_RN_NKConsigneeCountry = ZString.Empty;
				}
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				CS_MsgStatus = IsInDatabase ? (ZString)CS_MsgStatusInfo.OriginalValue : ZString.Empty;
				CS_fPartShipConsignmentReference = IsInDatabase ? (ZString)CS_fPartShipConsignmentReferenceInfo.OriginalValue : ZString.Empty;
			}
		}

		#endregion

		#region Property Overrides
		public override ZString CS_HAWB
		{
			get => base.CS_HAWB;
			set
			{
				var oldValue = base.CS_HAWB;
				base.CS_HAWB = value;
				if (value != oldValue)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnly(true)]
		public override ZString CS_TranshipmentEntryNum
		{
			get { return base.CS_TranshipmentEntryNum; }
		}

		public override ZString CS_CustomsStatus
		{
			get { return base.CS_CustomsStatus; }
			set
			{
				base.CS_CustomsStatus = value;
				if (value == CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed)
				{
					CS_IsMasterHouse = true;
				}
			}
		}

		[ReadOnly(true)]
		[RelatedBusinessObject(nameof(MAWB))]
		public override ZGuid CS_CM
		{
			get { return base.CS_CM; }
			set
			{
				bool valueChanged = CS_CM != value;
				base.CS_CM = value;
				if (valueChanged && !IsInDatabase)
				{
					Calculator.DeriveStatusNow();
				}
			}
		}

		public bool IsMasterBillChangedFromEmpty
		{
			get { return CS_CMInfo.OriginalValue.IsEmpty && CS_CM.IsValid; }
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = new StmNoteContexts();
				result.Direction |= StmNoteContextDirection.I;
				result.FreightMode |= StmNoteContextFreightMode.I;
				return result;
			}
		}

		[ReadOnlyMember(nameof(IsPrealertHeldByUserReadOnly))]
		public override ZBool CS_IsPrealertHeldByUser
		{
			get { return base.CS_IsPrealertHeldByUser; }
		}

		protected virtual bool IsPrealertHeldByUserReadOnly
		{
			get { return CS_IsPrealerted; }
		}

		[ReadOnly(true)]
		public override ZString CS_MessageReference
		{
			get { return base.CS_MessageReference; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CS_Weight
		{
			get { return base.CS_Weight; }
			set { base.CS_Weight = value; }
		}

		[DecimalPlaces(3)]
		public override ZDecimal CS_ChargableWeight
		{
			get { return base.CS_ChargableWeight; }
			set { base.CS_ChargableWeight = value; }
		}

		#endregion

		#region Party Details

		protected IOrgPatternLanguageSetting flanguageSetting;
		public IOrgPatternLanguageSetting AddressInfo
		{
			get
			{
				if (flanguageSetting == null)
				{
					flanguageSetting = OrgPatternLanguageSetting.Get(Enterprise.Core.Constants.Languages.English);
				}
				return flanguageSetting;
			}
		}

		const int AddressMaximumLength = Customs.Business.OrgAddressDecider.MaximumLength;
		const int PostcodeMaxLength = Customs.Business.OrgAddressDecider.PostcodeMaxLength;
		const int StateMaxLength = Customs.Business.OrgAddressDecider.StateMaxLength;
		const int CountryMaxLength = 2;

		#region DataRefresh on Organisations

		DataRefreshSubscriptionManager fConsignorSubscriptionManager;
		DataRefreshSubscriptionManager ConsignorSubscriptionManager
		{
			get
			{
				if (fConsignorSubscriptionManager == null)
				{
					fConsignorSubscriptionManager = new DataRefreshSubscriptionManager(new EventHandler(UpdateConsignorPartyDetails));
					fConsignorSubscriptionManager.BusinessObject = Consignor;
					fConsignorSubscriptionManager.Enabled = !ReadOnly;
				}
				return fConsignorSubscriptionManager;
			}
		}

		DataRefreshSubscriptionManager fConsigneeSubscriptionManager;
		DataRefreshSubscriptionManager ConsigneeSubscriptionManager
		{
			get
			{
				if (fConsigneeSubscriptionManager == null)
				{
					fConsigneeSubscriptionManager = new DataRefreshSubscriptionManager(new EventHandler(UpdateConsigneePartyDetails));
					fConsigneeSubscriptionManager.BusinessObject = Consignee;
					fConsigneeSubscriptionManager.Enabled = !ReadOnly;
				}
				return fConsigneeSubscriptionManager;
			}
		}

		void UpdateConsignorPartyDetails(object sender, EventArgs e)
		{
			RefreshBinding();
		}

		void UpdateConsigneePartyDetails(object sender, EventArgs e)
		{
			RefreshBinding();
		}

		#endregion

		#region Consignor
		public override OrgHeader Consignor => ConsignorAddress?.Header;

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.ConsignorList))]
		public ZGuid ConsignorOrgPK
		{
			get => CS_OA_ConsignorAddress_ZAddress.OrgPK;
			set => CS_OA_ConsignorAddress_ZAddress.OrgPK = value;
		}

		[List(nameof(CS_OA_ConsignorAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid CS_OA_ConsignorAddress
		{
			get => base.CS_OA_ConsignorAddress;
			set
			{
				if (base.CS_OA_ConsignorAddress != value)
				{
					base.CS_OA_ConsignorAddress = value;
					var consignorAddress = ConsignorAddress;
					UpdateConsignorDetails(Consignor, consignorAddress);

					base.CS_OH_Consignor = consignorAddress?.Header?.PK ?? ZGuid.Empty; // This is for keeping OH and OA fields in sync
				}
			}
		}

		protected override ZAddress GetNewCS_OA_ConsignorAddress_ZAddress()
		{
			var address = base.GetNewCS_OA_ConsignorAddress_ZAddress();
			address.GetDefaultAddress = header => GetDefaultConsignorAddress(header);
			return address;
		}

		protected IAddress fConsignorAddress;
		public IAddress ConsignorAddressForPortOfOrigin
		{
			get
			{
				IAddress result = null;
				if (Consignor != null)
				{
					result = new Customs.Business.PortBasedOrgAddressDecider(Consignor, Customs.Business.CargoAddressType.Pickup, delegate
					{
						return CS_RL_NKOrigin;
					});
				}
				return result;
			}
		}

		ZGuid GetDefaultConsignorAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			if (orgHeader is OrgHeader organisation)
			{
				var addressDecider = new Customs.Business.PortBasedOrgAddressDecider(organisation, Customs.Business.CargoAddressType.Delivery, delegate
				{
					return CS_RL_NKOrigin;
				});
				result = addressDecider.AddressWithFallback.PK;
			}
			return result;
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnly))]
		public override ZString CS_ConsignorName
		{
			get
			{
				return base.CS_ConsignorName.IsEmpty ? ConsignorAddress?.CompanyName.Left(CS_ConsigneeNameInfo.MaxLength) ?? ZString.Empty : base.CS_ConsignorName;
			}
			set { base.CS_ConsignorName = value; }
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnly))]
		public override ZString CS_ConsignorStreet
		{
			get
			{
				return base.CS_ConsignorStreet.IsEmpty ? ConsignorAddress?.Address1.Left(AddressMaximumLength) ?? ZString.Empty : base.CS_ConsignorStreet;
			}
			set { base.CS_ConsignorStreet = value; }
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnly))]
		public override ZString CS_ConsignorStreet2
		{
			get
			{
				return base.CS_ConsignorStreet2.IsEmpty ? ConsignorAddress?.Address2.Left(AddressMaximumLength) ?? ZString.Empty : base.CS_ConsignorStreet2;
			}
			set { base.CS_ConsignorStreet2 = value; }
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnly))]
		public override ZString CS_ConsignorCity
		{
			get
			{
				return base.CS_ConsignorCity.IsEmpty ? ConsignorAddress?.CityFallback.Left(AddressMaximumLength) ?? ZString.Empty : base.CS_ConsignorCity;
			}
			set { base.CS_ConsignorCity = value; }
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnly))]
		public override ZString CS_ConsignorState
		{
			get
			{
				return base.CS_ConsignorState.IsEmpty ? ConsignorAddress?.StateCode.Left(StateMaxLength) ?? ZString.Empty : base.CS_ConsignorState;
			}
			set { base.CS_ConsignorState = value; }
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnly))]
		public override ZString CS_ConsignorPostcode
		{
			get
			{
				return base.CS_ConsignorPostcode.IsEmpty ? ConsignorAddress?.Postcode.Left(PostcodeMaxLength) ?? ZString.Empty : base.CS_ConsignorPostcode;
			}
			set { base.CS_ConsignorPostcode = value; }
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnly))]
		public override ZString CS_RN_NKConsignorCountry
		{
			get
			{
				return base.CS_RN_NKConsignorCountry.IsEmpty ? ConsignorAddress?.OA_RL_NKRelatedPortCode.Left(CountryMaxLength) ?? ZString.Empty : base.CS_RN_NKConsignorCountry;
			}
			set { base.CS_RN_NKConsignorCountry = value; }
		}

		public override ZGuid CS_OH_Consignor
		{
			get { return base.CS_OH_Consignor; }
			set
			{
				if (base.CS_OH_Consignor != value)
				{
					base.CS_OH_Consignor = value;
					UpdateConsignorDetails(Consignor);
				}
			}
		}

		void UpdateConsignorDetails(OrgHeader consignor, OrgAddress consignorAddress = null)
		{
			ConsignorSubscriptionManager.BusinessObject = consignor;
			CS_ConsignorIdentifier = CargoHelper.GetIdentifier(consignor, consignorAddress);
			CS_VendorIdentifier = CargoHelper.GetConsignorVendor(consignor);
			OnSavingConsignorCheck();
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnly))]
		public override ZString CS_ConsignorPhone
		{
			get
			{
				return base.CS_ConsignorPhone.IsEmpty ? ConsignorAddress?.OA_Phone.Left(CS_ConsignorPhoneInfo.MaxLength) ?? ZString.Empty : base.CS_ConsignorPhone;
			}
			set { base.CS_ConsignorPhone = value; }
		}

		public ZString CS_ConsignorPhone_Formatted
		{
			get { return PhoneNumberPropertyHelperOfConsignor.GetPhoneNumber(CS_ConsignorPhoneInfo); }
		}

		public ZPropertyInfo CS_ConsignorPhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.CS_ConsignorPhone); }
		}

		protected virtual bool IsConsignorDetailsReadOnly
		{
			get { return Consignor != null; }
		}

		protected virtual bool IsConsignorDetailsReadOnlyUnlessUnmatched
		{
			get { return Consignor != null && Consignor.PK != OrgHeader.UnmatchedOrganisationPK; }
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnlyUnlessUnmatched))]
		public override ZString CS_ConsigneeBusinessNumber
		{
			get { return base.CS_ConsigneeBusinessNumber; }
			set { base.CS_ConsigneeBusinessNumber = value; }
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnlyUnlessUnmatched))]
		public override ZString CS_ConsigneeIdentifier
		{
			get { return base.CS_ConsigneeIdentifier; }
			set { base.CS_ConsigneeIdentifier = value; }
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnlyUnlessUnmatched))]
		public override ZString CS_ConsignorIdentifier
		{
			get { return base.CS_ConsignorIdentifier; }
			set { base.CS_ConsignorIdentifier = value; }
		}

		[ReadOnlyMember(nameof(IsConsignorDetailsReadOnlyUnlessUnmatched))]
		public override ZString CS_VendorIdentifier
		{
			get { return base.CS_VendorIdentifier; }
			set { base.CS_VendorIdentifier = value; }
		}

		public void SetConsignorDetails(IAddress consignorAddress)
		{
			if (consignorAddress != null)
			{
				CS_OA_ConsignorAddress = consignorAddress.OrgAddress?.PK ?? ZGuid.Empty;
				CS_ConsignorName = consignorAddress.CompanyName.Left(CS_ConsignorNameInfo.MaxLength);
				CS_ConsignorStreet = consignorAddress.Address1.Left(CS_ConsignorStreetInfo.MaxLength);
				CS_ConsignorStreet2 = consignorAddress.Address2.Left(CS_ConsignorStreet2Info.MaxLength);
				CS_ConsignorCity = consignorAddress.City.Left(CS_ConsignorCityInfo.MaxLength);
				CS_ConsignorPostcode = consignorAddress.PostCode.Left(CS_ConsignorPostcodeInfo.MaxLength);
				CS_ConsignorState = consignorAddress.State.Left(CS_ConsignorStateInfo.MaxLength);
				CS_ConsignorPhone = consignorAddress.Phone.Left(CS_ConsignorPhoneInfo.MaxLength);
				CS_RN_NKConsignorCountry = consignorAddress.CountryCode.Left(CS_RN_NKConsignorCountryInfo.MaxLength);
			}
		}

		#endregion

		#region Consignee

		public override OrgHeader Consignee => ConsigneeAddress?.Header;

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.ConsigneeList))]
		public ZGuid ConsigneeOrgPK
		{
			get => CS_OA_ConsigneeAddress_ZAddress.OrgPK;
			set => CS_OA_ConsigneeAddress_ZAddress.OrgPK = value;
		}

		[List(nameof(CS_OA_ConsigneeAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid CS_OA_ConsigneeAddress
		{
			get => base.CS_OA_ConsigneeAddress;
			set
			{
				if (base.CS_OA_ConsigneeAddress != value)
				{
					base.CS_OA_ConsigneeAddress = value;
					var consigneeAddress = ConsigneeAddress;
					UpdateConsigneeDetails(Consignee, consigneeAddress);

					base.CS_OH_Consignee = consigneeAddress?.Header?.PK ?? ZGuid.Empty; // This is for keeping OH and OA fields in sync
				}
			}
		}

		protected override ZAddress GetNewCS_OA_ConsigneeAddress_ZAddress()
		{
			var address = base.GetNewCS_OA_ConsigneeAddress_ZAddress();
			address.GetDefaultAddress = header => GetDefaultConsigneeAddress(header);
			return address;
		}

		public IAddress ConsigneeAddressForPortOfDestination
		{
			get
			{
				IAddress result = null;
				if (Consignee != null)
				{
					result = new Customs.Business.PortBasedOrgAddressDecider(Consignee, Customs.Business.CargoAddressType.Delivery, delegate
					{
						return CS_RL_NKDestination;
					});
				}
				return result;
			}
		}

		ZGuid GetDefaultConsigneeAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			if (orgHeader is OrgHeader organisation)
			{
				var addressDecider = new Customs.Business.PortBasedOrgAddressDecider(organisation, Customs.Business.CargoAddressType.Delivery, delegate
				{
					return CS_RL_NKDestination;
				});
				result = addressDecider.AddressWithFallback.PK;
			}
			return result;
		}

		public override ZGuid CS_OH_Consignee
		{
			get { return base.CS_OH_Consignee; }
			set
			{
				if (base.CS_OH_Consignee != value)
				{
					base.CS_OH_Consignee = value;
					UpdateConsigneeDetails(Consignee);
				}
			}
		}

		void UpdateConsigneeDetails(OrgHeader consignee, OrgAddress consigneeAddress = null)
		{
			ConsigneeSubscriptionManager.BusinessObject = consignee;

			var cid = CargoHelper.GetIdentifier(consignee, consigneeAddress);
			var businessNumber = CargoHelper.GetConsigneeBusinessNumber(consignee);
			CS_ConsigneeBusinessNumber = !businessNumber.IsEmpty && !cid.IsEmpty ? ZString.Empty : businessNumber;
			CS_ConsigneeIdentifier = cid;

			OnSavingConsigneeCheck();
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnly))]
		public override ZString CS_ConsigneeName
		{
			get
			{
				return base.CS_ConsigneeName.IsEmpty ? (ConsigneeAddress?.CompanyName.Left(CS_ConsigneeNameInfo.MaxLength) ?? ZString.Empty) : base.CS_ConsigneeName;
			}
			set { base.CS_ConsigneeName = value; }
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnly))]
		public override ZString CS_ConsigneeStreet
		{
			get
			{
				return base.CS_ConsigneeStreet.IsEmpty ? (ConsigneeAddress?.Address1.Left(AddressMaximumLength) ?? ZString.Empty) : base.CS_ConsigneeStreet;
			}
			set { base.CS_ConsigneeStreet = value; }
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnly))]
		public override ZString CS_ConsigneeStreet2
		{
			get
			{
				return base.CS_ConsigneeStreet2.IsEmpty ? (ConsigneeAddress?.Address2.Left(AddressMaximumLength) ?? ZString.Empty) : base.CS_ConsigneeStreet2;
			}
			set { base.CS_ConsigneeStreet2 = value; }
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnly))]
		public override ZString CS_ConsigneeCity
		{
			get
			{
				return base.CS_ConsigneeCity.IsEmpty ? (ConsigneeAddress?.CityFallback.Left(AddressMaximumLength) ?? ZString.Empty) : base.CS_ConsigneeCity;
			}
			set { base.CS_ConsigneeCity = value; }
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnly))]
		public override ZString CS_ConsigneeState
		{
			get
			{
				return base.CS_ConsigneeState.IsEmpty ? (ConsigneeAddress?.StateCode.Left(StateMaxLength) ?? ZString.Empty) : base.CS_ConsigneeState;
			}
			set { base.CS_ConsigneeState = value; }
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnly))]
		public override ZString CS_ConsigneePostcode
		{
			get
			{
				return base.CS_ConsigneePostcode.IsEmpty ? (ConsigneeAddress?.Postcode.Left(PostcodeMaxLength) ?? ZString.Empty) : base.CS_ConsigneePostcode;
			}
			set { base.CS_ConsigneePostcode = value; }
		}

		[ReadOnlyMember(nameof(IsConsigneeDetailsReadOnly))]
		public override ZString CS_RN_NKConsigneeCountry
		{
			get
			{
				return base.CS_RN_NKConsigneeCountry.IsEmpty ? (ConsigneeAddress?.OA_RL_NKRelatedPortCode.Left(CountryMaxLength) ?? ZString.Empty) : base.CS_RN_NKConsigneeCountry;
			}
			set { base.CS_RN_NKConsigneeCountry = value; }
		}

		protected virtual bool IsCS_ConsigneePhoneReadonly
		{
			get { return IsConsigneeDetailsReadOnly; }
		}

		[ReadOnlyMember(nameof(IsCS_ConsigneePhoneReadonly))]
		public override ZString CS_ConsigneePhone
		{
			get
			{
				return base.CS_ConsigneePhone.IsEmpty ? (ConsigneeAddress?.OA_Phone.Left(CS_ConsigneePhoneInfo.MaxLength) ?? ZString.Empty) : base.CS_ConsigneePhone;
			}
			set { base.CS_ConsigneePhone = value; }
		}

		public ZString CS_ConsigneePhone_Formatted
		{
			get { return PhoneNumberPropertyHelperOfConsignee.GetPhoneNumber(CS_ConsigneePhoneInfo); }
		}

		public ZPropertyInfo CS_ConsigneePhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.CS_ConsigneePhone); }
		}

		public virtual void SetConsigneeDetails(IAddress consigneeAddress)
		{
			if (consigneeAddress != null)
			{
				CS_OA_ConsigneeAddress = consigneeAddress.OrgAddress?.PK ?? ZGuid.Empty;
				CS_ConsigneeName = consigneeAddress.CompanyName.Left(CS_ConsigneeNameInfo.MaxLength);
				CS_ConsigneeStreet = consigneeAddress.Address1.Left(CS_ConsigneeStreetInfo.MaxLength);
				CS_ConsigneeStreet2 = consigneeAddress.Address2.Left(CS_ConsigneeStreet2Info.MaxLength);
				CS_ConsigneeCity = consigneeAddress.City.Left(CS_ConsigneeCityInfo.MaxLength);
				CS_ConsigneePostcode = consigneeAddress.PostCode.Left(CS_ConsigneePostcodeInfo.MaxLength);
				CS_ConsigneeState = consigneeAddress.State.Left(CS_ConsigneeStateInfo.MaxLength);
				CS_RN_NKConsigneeCountry = consigneeAddress.CountryCode.Left(CS_RN_NKConsigneeCountryInfo.MaxLength);
				CS_ConsigneePhone = consigneeAddress.Phone.Left(CS_ConsigneePhoneInfo.MaxLength);
			}
		}

		protected virtual bool IsConsigneeDetailsReadOnly
		{
			get { return Consignee != null; }
		}

		protected virtual bool IsConsigneeDetailsReadOnlyUnlessUnmatched
		{
			get { return Consignee != null && Consignee.PK != OrgHeader.UnmatchedOrganisationPK; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public new CusMAWBBase MAWB
		{
			get { return (CusMAWBBase)base.MAWB; }
		}

		public LogsForNominatedEvent PrealertHeldByUsers
		{
			get
			{
				if (fPrealertHeldByUsers == null)
				{
					fPrealertHeldByUsers = new LogsForNominatedEvent(Logs, Events.HoldAwaiting);
				}
				return fPrealertHeldByUsers;
			}
		}
		LogsForNominatedEvent fPrealertHeldByUsers;

		[ChildEditable(false)]
		public virtual CusPartShipCollection PartShips
		{
			get
			{
				if (fPartShips == null)
				{
					fPartShips = new CusPartShipCollection(this, Factory);
					fPartShips.Load();
					RegisterEditableChildObject(fPartShips);
				}
				return fPartShips;
			}
		}
		protected CusPartShipCollection fPartShips;

		#endregion

		#region ICusUnderbondDependentCollectionParent

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds => Underbonds;
		[ChildEditable(false)]
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.CountChanged += new CollectionCountChangedEventHandler(fUnderbonds_CountChanged);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		void fUnderbonds_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CusUnderbond underbond = e.BizObject as CusUnderbond;
			if (e.ItemAdded && underbond != null && MAWB != null)
			{
				underbond.C4_IsBureau = MAWB.CM_IsBureau;
			}
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get
			{
				CMREdiMessageFunctions messageFunctions = new CMREdiMessageFunctions();
				return messageFunctions.DoMessagesContainAnyCARSTs(Messages);
			}
		}

		protected abstract ZString DetailsCore
		{
			get;
		}

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get { return DetailsCore; }
		}

		protected abstract ZString UnderbondHumanReadableNameCore
		{
			get;
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get
			{
				return new IOutturnableLine[] { this };
			}
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return false; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		bool Customs.Business.IStatusNeedsRecalculationProvider.StatusNeedsRecalculation
		{
			get
			{
				bool result = false;
				if (!IsDeleted && MAWB != null && !MAWB.IsDeleted)
				{
					result = (HasMessagesBeenLoaded && Messages.HasChanges);
				}
				return result;
			}
		}

		#endregion

		#region ICusUnderbondUnionCollectionParent

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return true; }
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return DocumentSupporter; }
		}

		protected virtual CusHAWBDocumentSupporter DocumentSupporter
		{
			get { return new CusHAWBDocumentSupporter(this); }
		}

		public virtual bool RequiresDocumentUDFPlugIn
		{
			get { return false; }
		}

		#endregion

		#region ISACLiabilityQuestionProvider

		public ZDecimal GoodsValueInLocalCurrency
		{
			get
			{
				ZDecimal result = CS_GoodsValue;
				RefCurrency goodsCurr = GoodsCurrency;
				if (goodsCurr != null)
				{
					result = CurrencyConverter.ConvertExact(new Money(CS_GoodsValue, goodsCurr), JobDeclaration.GetLocalCurrency()).Amount;
				}
				return result;
			}
		}

		CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					fCurrencyConverter = new CurrencyConverterWithDataProvider(Factory, this);
				}
				return fCurrencyConverter;
			}
		}
		CurrencyConverter fCurrencyConverter;

		public ZString GoodsDescription
		{
			get { return CS_GoodsDescription; }
		}

		public ZPropertyInfo SACFlagInfo
		{
			get { return CS_IsSelfAssessedClearanceInfo; }
		}

		#endregion

		#region ICurrencyConverterDataProvider Members

		ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return Enterprise.ZArchitecture.Core.ExchangeRateType.Customs; }
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return 7; }
		}

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get { return MAWB != null ? MAWB.CM_DepartureDate : ZDateTime.Empty; }
		}

		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get { return MAWB != null && MAWB.Branch != null ? MAWB.Branch.Company : GlbCompany.CurrentCompany; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return ZString.Empty; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return null; }
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && CMRStatusHelper.CanDelete(CMRMessageStatus.Code);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return !CMRStatusHelper.CanDelete(CMRMessageStatus.Code) ? HAWBCannotBeDeleted : base.ReasonForNotAbleToDelete; }
		}

		internal static MultilingualString HAWBCannotBeDeleted
		{
			get
			{
				return ResString.GetMultilingualString("B6F03487-BDE9-4271-8A22-6ED06145EC61", "This HAWB cannot be deleted because there are messages associated with it.");
			}
		}

		#endregion // ICanDelete

		protected virtual void AddAHoldPrealertLogIfNecessary()
		{
			if (IsPrealertHeldByUsersSetBeforeInDatabase || IsPrealertHeldByUsersReset)
			{
				string reference = CS_IsPrealertHeldByUser ? Constants.HoldPrealertReference.IsPrealertHeld : Constants.HoldPrealertReference.IsPrealertHeldReleased;
				PrealertHeldByUsers.AddNew(reference);
			}
		}

		bool IsPrealertHeldByUsersSetBeforeInDatabase
		{
			get { return !IsInDatabase && CS_IsPrealertHeldByUser; }
		}

		bool IsPrealertHeldByUsersReset
		{
			get { return IsInDatabase && (ZBool)CS_IsPrealertHeldByUserInfo.OriginalValue != CS_IsPrealertHeldByUser; }
		}

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (allUnderbonds == null)
				{
					allUnderbonds = new CusUnderbondUnionCollection(this);
					allUnderbonds.Load();
				}
				return allUnderbonds;
			}
		}
		CusUnderbondUnionCollection allUnderbonds;

		ICusUnderbondDependentCollectionParent[] ICusUnderbondUnionCollectionParent.GetAllPossibleCollectionProviders()
		{
			return GetAllPossibleCollectionProvidersCore();
		}
		protected abstract ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProvidersCore();

		#endregion

		public new CusHAWBValidation Validation
		{
			get { return (CusHAWBValidation)base.Validation; }
		}

		protected override Customs.Business.CusHAWBValidation GetNewValidation()
		{
			throw new NotImplementedException("GetNewValidation() must be overridden on all subclasses of Enterprise.Customs.AU.AirCargo.Business.CusHAWBBase");
		}

		public new CusHAWBLookups Lookups
		{
			get { return (CusHAWBLookups)base.Lookups; }
		}

		protected override Customs.Business.CusHAWBLookups GetNewLookups()
		{
			return new CusHAWBLookups(this);
		}

		#region ITransitWarehouseSyncEventParent

		ZString ITransitWarehouseSyncEventParent.CustomsStatus => CS_CustomsStatus;

		#endregion
	}
}
