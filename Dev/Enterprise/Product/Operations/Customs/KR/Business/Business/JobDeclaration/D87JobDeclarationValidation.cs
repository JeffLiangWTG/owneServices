namespace Enterprise.Customs.KR.Business
{
	public class D87JobDeclarationValidation : JobDeclarationValidation
	{
		public D87JobDeclarationValidation(JobDeclaration declaration) : base(declaration)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDeclarantID();
			ValidateDeclarationCustomsDivision();
			ValidateRepresentativeProductName();
		}

		protected override void CheckJE_ShipmentIncoTerm()
		{
		}
		protected override void CheckPackagesActualPackageCount()
		{
		}
		protected override void CheckJE_DateOfArrival()
		{
		}
		protected override void CheckJE_TransportMode()
		{
		}
		protected override void CheckJE_MessageType()
		{
		}

		protected override void CheckJE_AgentsReference()
		{
			if (!Parent.JE_AgentsReference.IsEmpty)
			{
				var duplicateDeclaration = MessageLinkedObjectManager.GetDuplicateDeclarationD87(Parent.Factory, Parent.JE_GC, Parent.JE_AgentsReference, Parent.PK);
				if (duplicateDeclaration != null)
				{
					Parent.JE_AgentsReferenceInfo.AddMessageError(Res.GetString("4E90536D-B493-42A2-9FCE-9366FCCA5521", "[{0}] has this carnet certificate number. Please check if this number is correct.", duplicateDeclaration.JE_DeclarationReference));
				}
			}
		}

		protected override void CheckJE_OA_SupplierAddress() { }
		protected override void CheckJE_ExportGoodsType() { }
		protected override void CheckJE_CustomsOffice() { }
		protected override void CheckJE_TotalNoOfPieces() { }
		protected override void CheckJE_TotalWeight() { }
		protected override void CheckJE_TotalWeightUnit() { }
		protected override void CheckJE_TotalNoOfPacks() { }
		protected override void CheckJE_TotalNoOfPacksPackType() { }

		public void ValidateDeclarantID()
		{
			ValidateCalculatedProperty(Parent.UNIPASSDeclarantIDInfo);
		}
		protected void CheckDeclarantID() { }

		public void ValidateDeclarationCustomsDivision()
		{
			ValidateCalculatedProperty(Parent.JE_CustomsDivisionInfo);
		}
		protected void CheckDeclarationCustomsDivision() { }

		public void ValidateRepresentativeProductName()
		{
			ValidateCalculatedProperty(Parent.RepresentativeProductNameInfo);
		}
		protected void CheckRepresentativeProductName() { }

		protected override void CheckJE_CustomsDivision()
		{
		}

		protected override bool IsJE_ContainerPackModeMandatory => false;
	}
}
