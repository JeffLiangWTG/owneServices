using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public ImportAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override bool IsMandatoryZG_AgreedPlaceCodeCore => !((Parent.JobDeclaration?.IsUCC5 ?? false) && Parent.CusEntryInstructions.Cast<CusEntryInstruction>().All(x => x.IsH2 || x.IsI1));

		protected override void CheckZG_AgreedPlaceCode()
		{
			base.CheckZG_AgreedPlaceCode();
			CheckRuleBR4010();
		}

		void CheckRuleBR4010()
		{
			var parent = Parent;
			var agreedPlaceCode = parent.ZG_AgreedPlaceCode;
			var incoTerm = parent.JZ_IncoTerm;
			var incoTermsWithMessageError = new ZString[] { Core.Constants.IncoTerms.ExWorks, Core.Constants.IncoTerms.FreeCarrier, Core.Constants.IncoTerms.FreeAlongsideShip, Core.Constants.IncoTerms.FreeOnBoard, };
			if (!agreedPlaceCode.IsEmpty && incoTerm.In(incoTermsWithMessageError) && IsAgreedPlaceCodeStartsWithEUCountryCode(agreedPlaceCode))
			{
				parent.ZG_AgreedPlaceCodeInfo.AddMessageError(Res.GetString("37A790F1-9D07-4944-9E55-728D6EB67AF6", "[BR4010] Incoterm Place Code must be outside the EU."));
			}
		}

		bool IsAgreedPlaceCodeStartsWithEUCountryCode(ZString agreedPlaceCode)
		{
			if (agreedPlaceCode.Length < 2)
			{
				return false;
			}

			var euMemberProvider = ObjectFactory.Get<IEuropeanUnionCustomsMembersProvider>();
			return euMemberProvider.IsMemberOfEU(agreedPlaceCode.Substring(0, 2));
		}
	}
}
