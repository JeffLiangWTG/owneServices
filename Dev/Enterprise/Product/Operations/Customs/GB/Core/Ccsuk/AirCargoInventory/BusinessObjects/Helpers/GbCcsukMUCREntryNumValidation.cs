using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Helpers
{
	public class GbCcsukMUCREntryNumValidation : GbMUCREntryNumValidation, Integration.Customs.GB.CCSUK.IGbCcsukMUCREntryNumValidation
	{
		public GbCcsukMUCREntryNumValidation(CusEntryNumber parent)
			: base(parent)
		{
			gbDeclaration = base.Declaration;
		}

		readonly JobDeclaration gbDeclaration;

		public new JobDeclaration Declaration => gbDeclaration;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override void CheckCE_EntryNum()
		{
			base.CheckCE_EntryNum();
			var mucr = Declaration?.JE_MasterUCR ?? ZString.Empty;
			if (!mucr.IsEmpty)
			{
				var correctMucr = new ZString(new MasterUCRCalculator().Calculate(Declaration).TrimEnd());
				if (!correctMucr.IsEmpty && !mucr.IsEmpty && correctMucr != mucr)
				{
					Parent.CE_EntryNumInfo.AddMessageError(string.Format(CultureInfo.InvariantCulture, "Using the data available to {0}, the MUCR has been calculated as {1} but the value is {2}. Please check that your value is correct.", BrandingFactory.Instance.ProductName, correctMucr, mucr));
				}
			}
		}

		protected override void CountrySpecificValidation(ZString mucrNumber, ZPropertyInfo zPropertyInfo)
		{
			base.CountrySpecificValidation(mucrNumber, zPropertyInfo);

			if (Declaration != null && Declaration.IsImport)
			{
				// First try to get the AWB from its FK link(s)
				var baseHawbAndCount = FindBaseAwbFromFK(Declaration);
				var baseHawb = baseHawbAndCount.Item1;
				if (baseHawb != null)
				{
					// Success, found by FK
					var gbHawb = baseHawb as CusHAWB;
					if (gbHawb != null)
					{
						if (gbHawb.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.OnCommDb
							&& gbHawb.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection
							)
						{
							zPropertyInfo.AddWarning("The AWB that is linked to this declaration is not known to be on the CCSUK database.");
						}
						if (
								(gbHawb.CS_IsMasterHouse && (!mucrNumber.Contains(gbHawb.MAWB.CM_MAWB) || !mucrNumber.Contains(gbHawb.MAWB.CargoTerminalOperator)))
								||
								(!gbHawb.CS_IsMasterHouse && (!mucrNumber.Contains(gbHawb.MAWB.CM_MAWB) || !mucrNumber.Contains(gbHawb.CargoTerminalOperator) || !mucrNumber.Contains(gbHawb.CS_HAWB)))
							)
						{
							zPropertyInfo.AddMessageError(string.Format("The AWB that is linked to this declaration has a serial number that is not related to this MUCR. Its details are: {0} {1} {2}", (gbHawb.CS_IsMasterHouse ? gbHawb.MAWB.CargoTerminalOperator : gbHawb.CargoTerminalOperator), gbHawb.MAWB.CM_MAWB, gbHawb.CS_HAWB));
						}
						CheckConsignmentBadgeMatchesDeclarationBadgeAndOtherInventoryFields(gbHawb, zPropertyInfo);
					}
				}
				else
				{
					// not strongly linked to AWB, using NK instead
					if (Declaration.Shipment != null && IsCcsukModuleEnabled && Declaration.ZG_ShipmentType == EU.Business.ShipmentTypeList.Codes.HouseConsignment && baseHawbAndCount.Item2 == 0)
					{
						zPropertyInfo.AddMessageError("This declaration is linked to a shipment but there is no HAWB linked to it.");
					}
				}

				// Standalone
				var matches = MasterUCRHelper.PreparePatternMatch(mucrNumber);
				if (matches != null && matches.Count > 0)
				{
					if (IsCcsukModuleEnabled)
					{
						var optionalHawbFromMucr = MasterUCRHelper.GetMatchedCode(matches, "HAWB");
						var optionalSplitFromMucr = MasterUCRHelper.GetMatchedCode(matches, "SPLIT");
						var mawbOrBasic = MasterUCRHelper.FindMawbFromPatternMatch(matches, Declaration.Factory) as CusMAWB;
						if (mawbOrBasic != null)
						{
							if (optionalHawbFromMucr.IsEmpty)
							{
								// MUCR claims basic
								if (mawbOrBasic.IsBasic)
								{
									if (!optionalSplitFromMucr.IsEmpty && !mawbOrBasic.HasSplits)
									{
										zPropertyInfo.AddMessageError(string.Format("This MUCR specifies a split basic but AWB {0} has no splits.", mawbOrBasic.ReferenceNumber));
									}
									else if (optionalSplitFromMucr.IsEmpty && mawbOrBasic.HasSplits)
									{
										zPropertyInfo.AddMessageError(string.Format("This MUCR specifies a whole basic but AWB {0} has {1} splits.", mawbOrBasic.ReferenceNumber, mawbOrBasic.Splits.Count));
									}
									else if (!optionalSplitFromMucr.IsEmpty)
									{
										var split = mawbOrBasic.Splits[optionalSplitFromMucr];
										if (split == null)
										{
											zPropertyInfo.AddMessageError(string.Format("This MUCR specifies split {0} on basic {1}, which cannot be found.", optionalSplitFromMucr, mawbOrBasic.ReferenceNumber));
										}
										else
										{
											CheckConsignmentBadgeMatchesDeclarationBadgeAndOtherInventoryFields(split, zPropertyInfo);
										}
									}
								}
								else
								{
									zPropertyInfo.AddMessageError(string.Format("This MUCR specifies a basic but AWB {0} is a consolidation of {1} houses.", mawbOrBasic.ReferenceNumber, mawbOrBasic.ChildBills.Count));
								}

								if (IsAwbProbablyNotOnNetwork(mawbOrBasic))
								{
									zPropertyInfo.AddMessageError(string.Format("The AWB mentioned in the MUCR, {0}{1}, is not known to be on the CCS-UK network. Have you sent the FRI message?", mawbOrBasic.MasterLevelHouseHelper.CS_WarehouseLocation, mawbOrBasic.ReferenceNumber));
								}
								CheckConsignmentBadgeMatchesDeclarationBadgeAndOtherInventoryFields(mawbOrBasic, zPropertyInfo);
							}
							else
							{
								// MUCR mentions house
								var hawb = MasterUCRHelper.FindHawbFromPattern(optionalHawbFromMucr, mawbOrBasic.PK, Declaration.Factory) as CusHAWB;
								if (hawb == null)
								{
									zPropertyInfo.AddMessageError("No HAWB could be found from the MUCR.");
								}
								else
								{
									if (optionalSplitFromMucr.IsEmpty && hawb.HasSplits)
									{
										zPropertyInfo.AddMessageError(string.Format("This MUCR specifies a whole house but HAWB {0} has {1} splits.", hawb.ReferenceNumber, hawb.Splits.Count));
									}
									else if (!optionalSplitFromMucr.IsEmpty && !hawb.HasSplits)
									{
										zPropertyInfo.AddMessageError(string.Format("This MUCR specifies a split house but HAWB {0} has no splits.", hawb.ReferenceNumber));
									}
									else if (!optionalSplitFromMucr.IsEmpty && hawb.Splits[optionalSplitFromMucr] == null)
									{
										zPropertyInfo.AddMessageError(string.Format("This MUCR specifies split {0} on house {1}, which cannot be found.", optionalSplitFromMucr, hawb.ReferenceNumber));
									}
									else if (IsAwbProbablyNotOnNetwork(hawb))
									{
										zPropertyInfo.AddMessageError(string.Format("This MUCR specifies a house but HAWB {0} is not known to be on the CCS-UK network. Have you sent the FRI message?", hawb.ReferenceNumber));
									}

									if (!optionalSplitFromMucr.IsEmpty)
									{
										var split = hawb.Splits[optionalSplitFromMucr];
										if (split != null)
										{
											CheckConsignmentBadgeMatchesDeclarationBadgeAndOtherInventoryFields(split, zPropertyInfo);
										}
									}
									CheckConsignmentBadgeMatchesDeclarationBadgeAndOtherInventoryFields(hawb, zPropertyInfo);
								}
							}
						}
						else
						{
							zPropertyInfo.AddMessageError("No MAWB could be found from the MUCR.");
						}
					}
				}
				else
				{
					zPropertyInfo.AddMessageError("This MUCR appears to be invalid for a CCSUK air import. It should look like HBAC11122222222(33333333)(44).");
				}
			}
		}

		internal static Tuple<Customs.Business.CusHAWB, int> FindBaseAwbFromFK(JobDeclaration declaration)
		{
			var baseHawbs = declaration.Factory.Load<Customs.Business.CusHAWB>(new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, declaration.PK)).Take(2);
			if (!baseHawbs.Any() && declaration.Shipment != null)
			{
				baseHawbs = declaration.Factory.Load<Customs.Business.CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, declaration.Shipment.PK)).Take(2);
			}
			var count = baseHawbs.Count();
			return count == 1 ? new Tuple<Customs.Business.CusHAWB, int>(baseHawbs.Single(), 1) : new Tuple<Customs.Business.CusHAWB, int>(null, count);
		}

		void CheckConsignmentBadgeMatchesDeclarationBadgeAndOtherInventoryFields(ICcsukCusAwb awb, ZPropertyInfo zPropertyInfo)
		{
			var list = new List<ValidationHelper>();
			var isSplit = awb is SplitConsignment;
			if (!isSplit)
			{
				list.Add(new ValidationHelper(Declaration.JE_CustomsProfileInfo, awb.AgentBadge, null));
			}
			if (isSplit || !awb.HasSplits)
			{
				list.Add(new ValidationHelper(Declaration.JE_TotalNoOfPacksInfo, awb.NumberOfPiecesExpected, null));
			}
			if (!isSplit)
			{
				list.Add(new ValidationHelper(Declaration.SubLocationInfo, awb.CargoTerminalOperator, delegate
				{ return Declaration.JE_MasterUCR.SubstringSafe(1, 3) == awb.CargoTerminalOperator; }));
			}
			foreach (var trio in list)
			{
				if (trio.ValueOnAwb.ToString() != trio.ZpiOnDeclaration.Value.ToString())
				{
					if (trio.AdditionalValidationCheck != null)
					{
						if (trio.AdditionalValidationCheck())
						{
							continue;
						}
					}
					zPropertyInfo.AddMessageError(string.Format("This job references an inventory record whose {2} does not match this declaration's. AWB:{0}, Declaration:{1}", trio.ValueOnAwb, trio.ZpiOnDeclaration.Value, trio.ZpiOnDeclaration.HumanReadableName));
				}
			}
			if (awb.IsThroughAwb)
			{
				zPropertyInfo.AddMessageError(awb.HumanReadableName + " is a through AWB and should not be declared to CHIEF");
			}
		}

		bool IsAwbProbablyNotOnNetwork(ICcsukCusAwb awb)
		{
			return awb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck || awb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.NotOnCommDb || awb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.NotOnCommDbDeleted || awb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
		}

		bool IsCcsukModuleEnabled => true;

		class ValidationHelper : Tuple<ZPropertyInfo, IZType, Func<ZBool>>
		{
			public ValidationHelper(ZPropertyInfo zpi, IZType valueOnAwb, Func<ZBool> function)
				: base(zpi, valueOnAwb, function)
			{
			}
			public ZPropertyInfo ZpiOnDeclaration { get { return Item1; } }
			public IZType ValueOnAwb { get { return Item2; } }
			public Func<ZBool> AdditionalValidationCheck { get { return Item3; } }
		}
	}
}
