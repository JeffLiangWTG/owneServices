//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCAeMHMasterValidation
//
//    This class should be used for overriding validation in AutoCusCAeMHMasterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHMasterValidation : AutoCusCAeMHMasterValidation
	{
		public CusCAeMHMasterValidation(AutoCusCAeMHMaster parent) : base(parent)
		{
		}

		protected new CusCAeMHMaster Parent
		{
			get { return (CusCAeMHMaster)base.Parent; }
		}

		protected override void CheckBP_AmendReasonCode()
		{
			base.CheckBP_AmendReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BP_AmendReasonCodeInfo);
			if (Parent.IsPostArrival)
			{
				if (Parent.BP_AmendReasonCode.IsEmpty && Parent.IsLodged)
				{
					Parent.BP_AmendReasonCodeInfo.AddMessageError(Res.GetString("C066F3F7-A2C1-4E2F-A353-856EE0FCB9C7", "This consolidation has arrived and has been reported so if you submit an amendment then an amendment reason will be required."));
				}
			}
			else
			{
				var amendReasonCode = Parent.BP_AmendReasonCode;
				if (Parent.BP_ATA.IsEmpty && !amendReasonCode.IsEmpty)
				{
					Parent.BP_AmendReasonCodeInfo.AddMessageError(AmendReasonCodeIsNotAllowedError);
				}
				else if (amendReasonCode != EManifestAmendmentReasonCodes.Codes.PortOrSubLocation)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.BP_AmendReasonCodeInfo);
				}
			}
		}

		internal static string AmendReasonCodeIsNotAllowedError
		{
			get { return Res.GetString("2059c928-749e-4db6-bd98-02afc8bfc15e", "There is no Arrival message / event or the arrival date present – hence eManifest cannot be amended, if there is any change in information then please submit Change – not Amendment."); }
		}

		protected override void CheckBP_ModeOfTransport()
		{
			base.CheckBP_ModeOfTransport();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BP_ModeOfTransportInfo);
		}

		protected override void CheckBP_MasterBill()
		{
			base.CheckBP_MasterBill();
			MandatoryValidation.WarnIfNotEntered(Parent.BP_MasterBillInfo);
			CheckMasterBillMatchPrimaryCCN(Parent.BP_MasterBillInfo);
		}

		void CheckMasterBillMatchPrimaryCCN(ZPropertyInfo propertyInfo)
		{
			var masterBill = Parent.BP_MasterBill.ToUpper();
			var primaryCCN = Parent.BP_PrimaryCCN.ToUpper();
			var numberMatched = masterBill.Equals(primaryCCN);
			if (!numberMatched)
			{
				if (Parent.IsAir)
				{
					numberMatched = masterBill.Replace(masterBillOrCCNIgnoreChar, string.Empty)
						.Equals(primaryCCN.Replace(masterBillOrCCNIgnoreChar, string.Empty));
				}
				else if (masterBill.Length > masterBillOrCCNPrefixLength && primaryCCN.Length > masterBillOrCCNPrefixLength)
				{
					if (masterBill.Length == primaryCCN.Length)
					{
						numberMatched = masterBill.Substring(masterBillOrCCNPrefixLength).Equals(primaryCCN.SubstringSafe(masterBillOrCCNPrefixLength));
					}
					else if (masterBill.Length > primaryCCN.Length)
					{
						numberMatched = masterBill.SubstringSafe(masterBillOrCCNPrefixLength).Equals(primaryCCN);
					}
					else
					{
						numberMatched = masterBill.Equals(primaryCCN.SubstringSafe(masterBillOrCCNPrefixLength));
					}
				}
			}
			if (!numberMatched)
			{
				propertyInfo.AddWarning(MasterBillNotMatchPrimaryCCNMessage);
			}
		}

		readonly int masterBillOrCCNPrefixLength = 4;
		readonly string masterBillOrCCNIgnoreChar = "-";

		internal static string MasterBillNotMatchPrimaryCCNMessage => Res.GetString("197DF218-E025-4EB0-B096-18A2C7A0E04D", "Master Bill does not match Primary CCN, please confirm that this is correct.");

		protected override void CheckBP_PrimaryCCN()
		{
			base.CheckBP_PrimaryCCN();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BP_PrimaryCCNInfo);
			CheckCombinationOfPrimaryAndSubMasterCCNAreUnique(Parent.BP_PrimaryCCNInfo);

			if (Parent.BP_MasterHouseCCN.IsEmpty && Parent.IsLodged && Parent.BP_PrimaryCCNInfo.HasChanges)
			{
				Parent.BP_PrimaryCCNInfo.AddWarning(keyFieldError);
			}
			ValidateBP_MasterBill();
		}

		static string keyFieldError
		{
			get { return Res.GetString("e352a533-69ba-4b74-8d00-824ebbb9de39", "This is a key field and a Close message has already been filed. You must submit a withdrawal before you can change this field."); }
		}

		void CheckCombinationOfPrimaryAndSubMasterCCNAreUnique(ZPropertyInfo info)
		{
			if (!Parent.BP_PrimaryCCN.IsEmpty && !Parent.BP_MasterHouseCCN.IsEmpty)
			{
				var query = new ZQuery(CusCAeMHMasterSchema.BP_PrimaryCCN, Parent.BP_PrimaryCCN);
				query.AddToFilter(CusCAeMHMasterSchema.BP_MasterHouseCCN, Parent.BP_MasterHouseCCN);
				query.AddToFilter(CusCAeMHMasterSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				var billWithTheSameCombinationOfPrimaryAndSubCCN = Parent.Factory.LoadTop1<CusCAeMHMaster>(query);
				if (billWithTheSameCombinationOfPrimaryAndSubCCN != null)
				{
					info.AddMessageError(CombinationOfPrimaryAndSubCCNMustBeUnique(billWithTheSameCombinationOfPrimaryAndSubCCN.BP_MasterBill));
				}
			}
		}
		internal static string CombinationOfPrimaryAndSubCCNMustBeUnique(string masterBill)
		{
			return Res.GetString("19e95eb0-9f4d-4498-b1c9-94bac2e92b0e", "The combination of Primary CCN and Sub-Master CCN was used in Master Bill {0}", masterBill);
		}

		internal static string PeviousCCNDefaultControlledViaRegistrySetting => Res.GetString("CCA3AAAE-839D-4715-9284-8DECAD3AC8F4", "Invalid Previous Cargo Control Number. Either complete the Previous CCN or remove it. The Previous CCN default is controlled via registry setting Freight > Consolidations > Canada > Previous Cargo Control Number Customization.");
		internal const int PCCNLength = 4;

		protected override void CheckBP_MasterHouseCCN()
		{
			base.CheckBP_MasterHouseCCN();
			if (!Parent.BP_MasterHouseBill.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BP_MasterHouseCCNInfo);
			}

			var houseCCN = Parent.BP_MasterHouseCCN;
			if (!houseCCN.IsEmpty && houseCCN.Length <= PCCNLength)
			{
				Parent.BP_MasterHouseCCNInfo.AddMessageError(PeviousCCNDefaultControlledViaRegistrySetting);
			}

			CheckCombinationOfPrimaryAndSubMasterCCNAreUnique(Parent.BP_MasterHouseCCNInfo);

			if (Parent.IsLodged && Parent.BP_MasterHouseCCNInfo.HasChanges)
			{
				Parent.BP_MasterHouseCCNInfo.AddWarning(keyFieldError);
			}
		}

		protected override void CheckBP_CBSACarrierCode()
		{
			base.CheckBP_CBSACarrierCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BP_CBSACarrierCodeInfo);

			if (Parent.IsLodged && Parent.BP_CBSACarrierCodeInfo.HasChanges)
			{
				Parent.BP_CBSACarrierCodeInfo.AddWarning(keyFieldError);
			}

			if (!Parent.BP_CBSACarrierCode.IsEmpty)
			{
				var carrier = new Universal.ZZRefCarrierCombined.Loader(Parent.Factory).LoadFromCode(Enterprise.Core.Constants.CountryCodes.Canada, Parent.BP_CBSACarrierCode);
				if (carrier != null && !carrier.TransportModePairList.Any(x => x.Value && x.Description == Parent.BP_ModeOfTransport) && !carrier.HasMatchingAttributes(new[] { Parent.BP_ModeOfTransport }))
				{
					Parent.BP_CBSACarrierCodeInfo.AddWarning(Res.GetString("93CAF21F-979E-4CAF-8EFD-1E1AF5783FBB", "Carrier not allowed for mode of transport '{0}'", Parent.BP_ModeOfTransport));
				}
			}
		}

		protected override void CheckBP_CBSADischargePort()
		{
			base.CheckBP_CBSADischargePort();
			ListValidation.MessageErrorIfInvalidCode(Parent.BP_CBSADischargePortInfo);
			ValidateBP_CBSADischargeSubLocation();
		}

		protected override void CheckBP_CBSADischargeSubLocation()
		{
			base.CheckBP_CBSADischargeSubLocation();
			if (Parent.BP_CBSADischargeSubLocation.IsEmpty)
			{
				if (!Parent.BP_CBSADischargePort.IsEmpty)
				{
					Parent.BP_CBSADischargeSubLocationInfo.AddMessageError(Res.GetString("3D50F49E-E309-4C5C-8299-37E9CE9D0CFD", "Must be entered if Cust. Port of Discharge is entered."));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BP_CBSADischargeSubLocationInfo);
			}
		}

		protected override void CheckBP_RL_NKDiscPort()
		{
			base.CheckBP_RL_NKDiscPort();
			if (Parent.BP_RL_NKDiscPort.IsEmpty)
			{
				if (!Parent.BP_ATA.IsEmpty)
				{
					Parent.BP_RL_NKDiscPortInfo.AddMessageError(DiscPortIsMissing);
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BP_RL_NKDiscPortInfo);
			}
		}

		internal static string DiscPortIsMissing
		{
			get { return Res.GetString("b9b210a0-352b-4b07-8f83-5f900ac48646", "You have entered Arrival Time. You should enter Port Of Discharge as well."); }
		}
	}
}
