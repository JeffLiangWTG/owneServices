using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocRexAcknowledgementCollection))]
	sealed class QuarantineExDocRexAcknowledgementCollectionTest : CusCodeDataCollectionTest<QuarantineExDocRexAcknowledgement>
	{
		protected override CusCodeDataCollection<QuarantineExDocRexAcknowledgement> GetCusCodeDataCollection() => new QuarantineExDocRexAcknowledgementCollection(QuarantineHeader);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<QuarantineExDocRexAcknowledgement>();
			result.CY_ParentID = QuarantineHeader.PK;
			result.CY_ParentTableCode = QuarantineHeader.TablePrefix;
			return result;
		}

		QuarantineExDocHeader quarantineHeader;
		QuarantineExDocHeader QuarantineHeader => quarantineHeader ?? (quarantineHeader = Factory.New<QuarantineExDocHeader>());
	}
}
