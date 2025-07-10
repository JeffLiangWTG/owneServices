using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AUShipmentDeclarationGeneratorTest : ShipmentDeclarationGeneratorTest
	{
		public override void TestInvoiceGenerator()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			AssertEquals("Invoice generator type", ExpecteInvoiceGeneratorType, (new AUShipmentDeclarationGeneratorForTest()).GetNewInvoiceGenerator(jobDec).GetType());
		}

		protected override IShipmentDeclarationGenerator DeclarationGenerator
		{
			get
			{
				if (fDeclarationGenerator == null)
				{
					fDeclarationGenerator = new AUShipmentDeclarationGenerator();
				}

				return fDeclarationGenerator;
			}
		}

		IShipmentDeclarationGenerator fDeclarationGenerator;
		protected override Type TypeOfDeclaration
		{
			get
			{
				return typeof(JobDeclaration);
			}
		}

		protected override Type ExpecteInvoiceGeneratorType
		{
			get
			{
				return typeof(AUInvoicesGeneratorFromXSD);
			}
		}

		class AUShipmentDeclarationGeneratorForTest : AUShipmentDeclarationGenerator
		{
			public new InvoicesGeneratorFromXSD GetNewInvoiceGenerator(BaseJobDeclaration declaration)
			{
				return base.GetNewInvoiceGenerator(declaration);
			}
		}
	}
}
