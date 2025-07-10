using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public class CreateDeclarationIPR : CreateDeclarationBizObj
	{
		public CreateDeclarationIPR() : base(false)
		{
		}

		public new class Schema : AutoNonPersistentCreateDeclarationBizObj.Schema
		{
			public new const int DeclarantsReferenceMaxLength = 22;
		}

		[MaxLength(Schema.DeclarantsReferenceMaxLength)]
		public override ZString DeclarantsReference
		{
			get => base.DeclarantsReference;
			set => base.DeclarantsReference = value;
		}

		protected override CreateDeclarationBizObjLookups GetNewLookups() => new CreateDeclarationIPRLookups(this);

		protected override NonPersistentCreateDeclarationBizObjValidation GetNewValidation() => new NonPersistentCreateDeclarationIPRValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DeclarationType = ImportDeclarationTypeList.Codes.AVABR;
			CPC = ImportMainProcedureCodeList.Codes._40;
		}
	}
}
