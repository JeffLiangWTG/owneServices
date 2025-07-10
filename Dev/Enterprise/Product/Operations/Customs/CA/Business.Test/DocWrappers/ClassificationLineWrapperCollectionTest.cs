using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ClassificationLineWrapperCollection))]
	public class ClassificationLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ClassificationLineWrapperCollection>
	{
		protected override ClassificationLineWrapperCollection GetCollectionToTest()
		{
			var b3Header = new ExpectedB3HeaderForTesting(Factory);
			b3Header.PositiveClassificationLines = System.Array.Empty<IClassificationLine1>();
			return new ClassificationLineWrapperCollection(b3Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ClassificationLine1Wrapper
				(new JobComInvoiceLineTest.ExpectedClassificationLine1ForTesting());
		}

		public class ExpectedB3HeaderForTesting : IB3Header
		{
			public ExpectedB3HeaderForTesting(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}

			public ZString BatchNumber { get; set; }
			public ZString B3TypeCode { get; set; }
			public ZString PaymentCode { get; set; }
			public ZString CBSAOffice { get; set; }
			public ZString PortOfUnlading { get; set; }
			public ZString WarehouseNumber { get; set; }
			public ZString TransactionNumber { get; set; }
			public ZString BusinessNumber { get; set; }
			public ZString GSTNumber { get; set; }
			public ZString TransportMode { get; set; }
			public ZString CarrierCodeAtImportation { get; set; }
			public IEnumerable<IB3BRelease> B3BInputReleases { get; set; }
			public ZDecimal TotalValueForDuty { get; set; }
			public IEnumerable<IB3SubHeader> PositiveB3SubHeaders { get; set; }
			public IEnumerable<IClassificationLine1> PositiveClassificationLines { get; set; }
			public IEnumerable<IB3SubHeader> NegativeB3SubHeaders { get; set; }
			public IEnumerable<IClassificationLine1> NegativeClassificationLines { get; set; }
			public ITotalAmounts PositiveTotalAmounts { get; set; }
			public ITotalAmounts NegativeTotalAmounts { get; set; }
			public bool IsCalculationsDone { get; set; }
			public bool SumPosAndNeg { get; set; }
			public ZString B3Comments { get; set; }
			public ZDateTime ReleaseDate { get; set; }
			public MasterFiles.Integration.IDocAddress Importer { get; set; }
			public ZString AccountSecurityCode { get; set; }
			public bool IsCancelled { get; set; }
			public void AddMessage(Enterprise.Messaging.Business.EDIMessage message) { }

			public void RefreshCachedValues()
			{
			}

			public bool HasChanges { get; set; }
			public ZString JobIdentification { get; set; }
			public ZString JobStatus { get; set; }
			public ZString MessageStatus { get; set; }
			public BusinessObject TopLevelBusinessObject { get; set; }
			public BusinessObjectFactory Factory { get; set; }
			public Enterprise.Messaging.Business.EDIMessageCollection Messages { get; set; }

			public ZString MessageType { get; set; }

			public bool RefreshValidationBeforeSendMessage => true;
		}
	}
}
