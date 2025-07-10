using System.Linq;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(DeclarationH7MessageWrapper))]
	public class DeclarationH7MessageWrapperTest : H7CommonSendMessageWrapperBaseTest<DeclarationH7MessageWrapper>
	{
		public void TestHeader()
		{
			AssertType<DeclarationH7HeaderWrapper>(Provider.Header);
		}

		public void TestLines()
		{
			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();
			var item3 = bill.PackedItems.AddNew();

			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			var pack3 = bill.Packs.AddNew();

			item1.PackagesPivot.AddPivotFor(pack1);
			item1.PackagesPivot.AddPivotFor(pack2);
			item1.PackagesPivot.AddPivotFor(pack3);

			item2.PackagesPivot.AddPivotFor(pack1);
			item2.PackagesPivot.AddPivotFor(pack2);
			item3.PackagesPivot.AddPivotFor(pack1);

			var line1 = Provider.Lines.First(line => line.LineNumber == item1.SequenceNumber);
			var line2 = Provider.Lines.First(line => line.LineNumber == item2.SequenceNumber);
			var line3 = Provider.Lines.First(line => line.LineNumber == item3.SequenceNumber);

			CombineAssertions(() =>
			{
				AssertEquals(line1.NumberOfPackages, 3);
				AssertEquals(line2.NumberOfPackages, 0);
				AssertEquals(line3.NumberOfPackages, 0);
			});
		}

		protected override DeclarationH7MessageWrapper GetWrapperCore(AsycudaBill bill, ICertificateProvider certificate)
		{
			return new DeclarationH7MessageWrapper(bill, certificate);
		}
	}
}
