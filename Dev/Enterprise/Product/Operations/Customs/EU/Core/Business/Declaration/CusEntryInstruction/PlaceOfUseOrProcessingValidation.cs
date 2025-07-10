using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class PlaceOfUseOrProcessingValidation : CusGoodsLocationValidation
	{
		public PlaceOfUseOrProcessingValidation(PlaceOfUseOrProcessing parent) : base(parent)
		{
		}

		new PlaceOfUseOrProcessing Parent => (PlaceOfUseOrProcessing)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckDuplicates();
		}

		void CheckDuplicates()
		{
			var placeOfUseOrProcessing = Parent;

			if (placeOfUseOrProcessing.Instruction is CusEntryInstruction entryInstruction)
			{
				var currentPK = placeOfUseOrProcessing.PK;
				var displayText = placeOfUseOrProcessing.DisplayText;
				if (entryInstruction.PlaceOfUseOrProcessingCollection.Cast<PlaceOfUseOrProcessing>().Any(x => x.PK != currentPK && x.DisplayText == displayText))
				{
					placeOfUseOrProcessing.AddRowMessageError(Res.GetString("1522B4B9-EC2D-4774-8D49-A0356770D580", "Place must be unique, no duplicate allowed."));
				}
			}
		}

		protected override void CheckCGL_AdditionalIdentifier()
		{
		}

		protected override IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetCustomsOfficeRequirementRule()
			=> new Dictionary<string, (ZString, ZPropertyInfo)>
			{
				{ CusGoodsLocationQualifierList.Codes.UnLocode, (ValidationRuleCodeConstants.Codes.C0061, Parent.UnlocodeInfo) },
				{ CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, (ValidationRuleCodeConstants.Codes.C0062, Parent.CGL_CustomsOfficeInfo) },
			};

		protected override void ValidateCGL_CustomsOfficeInvalidCode(ZPropertyInfo propertyInfo)
		{
			if (Parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode)
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo, (IBusinessObjectCollection)Parent.Lookups.UnlocodeList);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo);
			}
		}
	}
}
