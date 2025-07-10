using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation cusGoodsLocation)
			: base(cusGoodsLocation)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateLoadingPlace();
		}

		new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		public override bool IncludeRuleCode => false;

		protected override void CheckCGL_Qualifier()
		{
			base.CheckCGL_Qualifier();
			
			var parent = Parent;
			var instruction = parent.Parent as Declaration.CusEntryInstruction;
			var jobDeclaration = instruction?.JobDeclaration;
			if (jobDeclaration != null && jobDeclaration.IsExport)
			{
				var qualifierOfIdentification = parent.CGL_Qualifier;
				var targetInfo = parent.CGL_QualifierInfo;
				ListValidation.MessageErrorIfInvalidCode(targetInfo);

				if (!qualifierOfIdentification.IsEmpty)
				{
					if (instruction.Style4thDigitIs1() || instruction.Style4thDigitIs9())
					{
						if (qualifierOfIdentification != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier)
						{
							targetInfo.AddMessageError(Res.GetString("4D7CAB75-375B-4302-B9A5-AC03ADC29E84", "Only Code 'V' can be selected."));
						}
					}
					else if (instruction.Style4thDigitIs2())
					{
						if (qualifierOfIdentification != CusGoodsLocationQualifierList.Codes.UnLocode
							&& qualifierOfIdentification != CusGoodsLocationQualifierList.Codes.GnssCoordinates
							&& qualifierOfIdentification != CusGoodsLocationQualifierList.Codes.AuthorizationNumber
							&& qualifierOfIdentification != CusGoodsLocationQualifierList.Codes.Address
							)
						{
							targetInfo.AddMessageError(Res.GetString("9309AC1D-9E7B-4C0A-AB9F-5664EBF06136", "Only Codes 'U', 'W', 'Y', 'Z' can be selected."));
						}
					}
					else if (instruction.Style4thDigitIs3() || instruction.Style4thDigitIs4())
					{
						if (qualifierOfIdentification != CusGoodsLocationQualifierList.Codes.AuthorizationNumber)
						{
							targetInfo.AddMessageError(Res.GetString("499ADD8A-F0D6-4C3F-BD5C-B9CB4174E87E", "Only Code 'Y' can be selected."));
						}
					}
				}
				else
				{
					if (instruction.Style2ndDigitIs0And3rdDigitIs1() || instruction.StyleFirstThreeDigitsAre1())
					{
						targetInfo.AddWarning(Res.GetString("A926DE2B-A32C-470F-A21B-C9E198A3C25C", "If your Authorization contains codes for loading place please use Qualifier Of identification 'Y'."));
					}
					else if (!ExportDeclarationTypeTimeList.IsMultipleDeclarationForExport(instruction.CEI_SubStyle) && !instruction.CEI_Style.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
					}
				}
			}
		}

		public void ValidateLoadingPlace()
		{
			ValidateCalculatedProperty(Parent.LoadingPlaceInfo);
		}

		protected void CheckLoadingPlace()
		{
			var parent = Parent;
			if (parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address)
			{
				var instruction = parent.Parent as CusEntryInstruction;
				if (instruction?.JobDeclaration?.IsExport ?? false)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.LoadingPlaceInfo);
				}
			}
		}

		protected override IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetCustomsOfficeRequirementRule() => new Dictionary<string, (ZString, ZPropertyInfo)>();

		protected override IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetAdditionalIdentifierRequirementRule()
		{
			var result =  base.GetAdditionalIdentifierRequirementRule();
			result[CusGoodsLocationQualifierList.Codes.AuthorizationNumber] = (ZString.Empty, Parent.CGL_AdditionalIdentifierInfo);
			return result;
		}
	}
}
