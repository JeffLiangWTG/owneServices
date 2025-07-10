using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class GenPivotTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_BoilerPlate()
		{
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.US.IFDARelatedBillsGenPivot>(), Decider.GetTypeForLoad(((INeedRow)UsFdaBills).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.US.IFDARelatedContainersGenPivot>(), Decider.GetTypeForLoad(((INeedRow)UsFdaContainers).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.US.IPGARelatedContainersGenPivot>(), Decider.GetTypeForLoad(((INeedRow)UsPgaContainers).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.IInvoiceRelatedDeclarationGenPivot>(), Decider.GetTypeForLoad(((INeedRow)InvoiceRelatedDeclarationGenPivot).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.IGroupRelatedDeclarationGenPivot>(), Decider.GetTypeForLoad(((INeedRow)GroupRelatedDeclarationGenPivot).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.IABLEntryNumRelatedPacksGenPivot>(), Decider.GetTypeForLoad(((INeedRow)ABCEntryNumRelatedPacksGenPivot).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IInvoiceLineRelatedCAHeadersGenPivot>(), Decider.GetTypeForLoad(((INeedRow)InvoiceLineRelatedCAHeadersGenPivot).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IDeclarationRelatedImportLicenseEntryGenPivot>(), Decider.GetTypeForLoad(((INeedRow)JobDeclarationRelatedImportLicenseEntryGenPivot).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IRelatedEntryInstructionGenPivot>(), Decider.GetTypeForLoad(((INeedRow)RelatedEntryInstructionGenPivot).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.CH.INctsRelatedArrivalGenPivot>(), Decider.GetTypeForLoad(((INeedRow)NctsRelatedArrivalGenPivot).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.CH.INctsRelatedExportEntryHeaderGenPivot>(), Decider.GetTypeForLoad(((INeedRow)NctsRelatedExportEntryHeaderGenPivot).Row, Factory));
			AssertEquals("Type", ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IAttachmentInvoiceLineGenPivot>(), Decider.GetTypeForLoad(((INeedRow)AttachmentInvoiceLineLink).Row, Factory));
		}

		public void TestGetTypeForLoad_ActuallyUseful()
		{
			var newFactory = new BusinessObjectFactory();

			void AssertGenPivotType<T>(string relationType)
			{
				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = relationType;

				Factory.Save();

				Assert($"Should be {typeof(T).FullName} when the relation type is {relationType}.", newFactory.Load<GenPivot>(pivot.PK) is T);
			}

			CombineAssertions(() =>
			{
				AssertGenPivotType<Enterprise.Integration.Customs.US.IPGARelatedContainersGenPivot>(GenPivotTypeDecider.Types.PGARelatedContainersGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.US.IFDARelatedBillsGenPivot>(GenPivotTypeDecider.Types.FDARelatedBillsGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.US.IFDARelatedContainersGenPivot>(GenPivotTypeDecider.Types.FDARelatedContainersGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.IInvoiceRelatedDeclarationGenPivot>(GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.IGroupRelatedDeclarationGenPivot>(GenPivotTypeDecider.Types.GroupRelatedDeclarationGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.ASYCUDA.IABLEntryNumRelatedPacksGenPivot>(GenPivotTypeDecider.Types.ABCEntryNumRelatedPacksGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.TW.IInvoiceLineRelatedCAHeadersGenPivot>(GenPivotTypeDecider.Types.InvoiceLineRelatedControllingMessageHeaderPivot);
				AssertGenPivotType<GenPivot>(GenPivotTypeDecider.Types.InvoiceLineRelatedTrademarkImagePivot);
				AssertGenPivotType<Enterprise.Integration.Customs.BR.IDeclarationRelatedImportLicenseEntryGenPivot>(GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.BR.IRelatedEntryInstructionGenPivot>(GenPivotTypeDecider.Types.RelatedEntryInstructionGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.CH.INctsRelatedArrivalGenPivot>(GenPivotTypeDecider.Types.NctsRelatedArrivalGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.CH.INctsRelatedExportEntryHeaderGenPivot>(GenPivotTypeDecider.Types.NctsRelatedExportGenPivot);
				AssertGenPivotType<Enterprise.Integration.Customs.CN.IAttachmentInvoiceLineGenPivot>(GenPivotTypeDecider.Types.AttachmentInvoiceLineLink);
			});

			var cusNctsContainerPivot = Factory.New<GenPivot>();
			cusNctsContainerPivot.XX_RelationType = GenPivotTypeDecider.Types.CusNctsContainer;
			cusNctsContainerPivot.XX_Relation1TableCode = CusInvPackSchema.Constants.Prefix;

			Factory.Save();

			AssertNotNull(newFactory.Load<GenPivot>(cusNctsContainerPivot.PK) as Enterprise.Integration.Customs.EU.NCTS.INctsCusInBondContainerPackageGenPivot);
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals("Type", null, Decider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals("Type", null, Decider.GetTypeForNew());
		}

		GenPivotTypeDecider Decider
		{
			get { return new GenPivotTypeDecider(); }
		}

		GenPivot UsFdaBills
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.US.IFDARelatedBillsGenPivot>(); }
		}

		GenPivot UsFdaContainers
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.US.IFDARelatedContainersGenPivot>(); }
		}

		GenPivot UsPgaContainers
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.US.IPGARelatedContainersGenPivot>(); }
		}

		GenPivot InvoiceRelatedDeclarationGenPivot
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.IInvoiceRelatedDeclarationGenPivot>(); }
		}

		GenPivot GroupRelatedDeclarationGenPivot
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.IGroupRelatedDeclarationGenPivot>(); }
		}

		GenPivot ABCEntryNumRelatedPacksGenPivot
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.ASYCUDA.IABLEntryNumRelatedPacksGenPivot>(); }
		}

		GenPivot InvoiceLineRelatedCAHeadersGenPivot
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.TW.IInvoiceLineRelatedCAHeadersGenPivot>(); }
		}

		GenPivot JobDeclarationRelatedImportLicenseEntryGenPivot
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.BR.IDeclarationRelatedImportLicenseEntryGenPivot>(); }
		}

		GenPivot RelatedEntryInstructionGenPivot
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.BR.IRelatedEntryInstructionGenPivot>(); }
		}

		GenPivot NctsRelatedArrivalGenPivot
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.CH.INctsRelatedArrivalGenPivot>(); }
		}

		GenPivot NctsRelatedExportEntryHeaderGenPivot
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.CH.INctsRelatedExportEntryHeaderGenPivot>(); }
		}

		GenPivot AttachmentInvoiceLineLink
		{
			get { return (GenPivot)Factory.New<Enterprise.Integration.Customs.CN.IAttachmentInvoiceLineGenPivot>(); }
		}
	}
}
