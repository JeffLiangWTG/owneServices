using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.DIF.Business.Testing;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Business
{
	sealed class MessageManagerTest : Customs.Business.Testing.DISMessageManagerBaseTest<DIFDocument>
	{
		[TestDate(2017, 1, 2)]
		public override void TestSendSubmission()
		{
			using (Factory.AddDisposableService())
			{
				DIFDocument.Status = StatusList.Codes.RejectedOriginal;
				new MessageManager(DIFDocument).SendSubmission();
				AssertEquals(StatusList.Codes.AwaitingChange, DIFDocument.RequiredDocumentAddInfo.EX_Status);

				DIFDocument.Status = ZString.Empty;
				new MessageManager(DIFDocument).SendSubmission();
				AssertEquals(StatusList.Codes.AwaitingOriginal, DIFDocument.RequiredDocumentAddInfo.EX_Status);

				DIFDocument.Status = StatusList.Codes.AwaitingWithdrawal;
				new MessageManager(DIFDocument).SendSubmission();
				AssertEquals(StatusList.Codes.AwaitingChange, DIFDocument.RequiredDocumentAddInfo.EX_Status);

				JobDeclaration[JobDeclarationSchema.JE_DateOfArrival.Name] = new ZDate(2017, 1, 1);
				new MessageManager(DIFDocument).SendSubmission();
				AssertEquals(StatusList.Codes.AwaitingAmendment, DIFDocument.RequiredDocumentAddInfo.EX_Status);
			}
		}

		public override void TestSendWithdrawal()
		{
			using (Factory.AddDisposableService())
			{
				new MessageManager(DIFDocument).SendWithdrawl();
				AssertEquals(StatusList.Codes.AwaitingWithdrawal, DIFDocument.RequiredDocumentAddInfo.EX_Status);
			}
		}

		protected override DISMessageManagerBase GetMessageManager(IDISDocumentBase disDocument) => new MessageManager(disDocument as DIFDocument);

		DIFHostWrapper hostWrapper;
		protected override DISHostWrapperBase<DIFDocument> HostWrapper => hostWrapper ?? (hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration));

		BusinessObject jobDeclaration;
		protected override BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			DbConnection connection = ((IDbConnected)Factory).Connection;
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("GeneratorFountain-C-8E86F7C5", Guid.Empty);
				ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("GeneratorFountain-C-7BE2F10C", Guid.Empty);

				var companyFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345");
				companyFountain.SetValues(Factory, 1000, 1000, 2000);
				var difFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345DIF");
				difFountain.SetValues(Factory, 777, 777, 2000);
				transactionManager.CommitTransaction();
			}
		}

		DIFDocument DIFDocument
		{
			get
			{
				if (difDocument == null)
				{
					var declaration = (IDISHost)JobDeclaration;
					var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
					var addInfo = Factory.New<JobRequiredDocumentAddInfo>();
					addInfo.EX_GC_Company = GlbCompany.CurrentCompany.PK;
					addInfo.EX_ApplicationCode = "CAD";
					addInfo.EX_EQ_RequiredDocument = requiredDocument.PK;
					difDocument = new DIFDocument(HostWrapper as DIFHostWrapper);
					difDocument.RequiredDocumentAddInfo = addInfo;
				}
				return difDocument;
			}
		}
		DIFDocument difDocument;
	}
}
