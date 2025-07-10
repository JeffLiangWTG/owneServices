using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[RootElement("UniversalTransactionBatch")]
	[XsdSchema("UniversalTransactionBatch.xsd")]
	public partial class TransactionBatch : TopLevelDataObject, IOrganizationAddressCollectionParent
	{
		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(Universal._2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(Universal._2012_11.DataContext))]
		public override IDataContextDataObject DataContext { get; set; }
		public List<TransactionInfo> TransactionCollection { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; private set; }

		public CodeDescriptionPair BatchType { get; set; }
		public CodeDescriptionPair Periodicity { get; set; }
		public ZDateTime? DateFrom { get; set; }
		public ZDateTime? DateTo { get; set; }

		public TransactionBatch()
		{
			TransactionCollection = new List<TransactionInfo>();
		}

		public TransactionBatch(IDataObjectWriterStrategy writerStrategy)
			: this()
		{
			SetWriterStrategy(writerStrategy);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && TransactionCollection != null)
			{
				foreach (var transaction in TransactionCollection)
				{
					transaction.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}

	public static class TransactionBatchOrganizationType
	{
		public const string SendingCompany = nameof(SendingCompany);
		public const string SendingBranch = nameof(SendingBranch);
	}
}

