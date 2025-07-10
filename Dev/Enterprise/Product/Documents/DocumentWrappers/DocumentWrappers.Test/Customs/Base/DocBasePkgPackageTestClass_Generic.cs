using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(DocBasePkgPackage))]
	public abstract class DocBasePkgPackageTestClass<T, TWrapper> : DocumentWrapperTestCase
		where T : CusPackage
		where TWrapper : DocBasePkgPackage
	{
		#region Abstract

		protected abstract TWrapper CreatePkgPackageWrapper(T package);

		#endregion

		#region ZDecimal Fields

		public void TestWeight()
		{
			AssertEquals("Weight", PkgPackageInternal.KP_Weight, PkgPackageWrapperInternal.Weight);
		}

		public void TestLength()
		{
			AssertEquals("Length", PkgPackageInternal.KP_Length, PkgPackageWrapperInternal.Length);
		}

		public void TestWidth()
		{
			AssertEquals("Width", PkgPackageInternal.KP_Width, PkgPackageWrapperInternal.Width);
		}

		public void TestHeight()
		{
			AssertEquals("Height", PkgPackageInternal.KP_Height, PkgPackageWrapperInternal.Height);
		}

		public void TestVolume()
		{
			AssertEquals("Volume", PkgPackageInternal.KP_Volume, PkgPackageWrapperInternal.Volume);
		}

		public void TestNetWeight()
		{
			AssertEquals("Net Weight", PkgPackageInternal.NetWeight, PkgPackageWrapperInternal.NetWeight);
		}

		#endregion

		#region ZInt Properties

		public void TestPackageQty()
		{
			AssertEquals("PackageQty", PkgPackageInternal.KP_PackageQty, PkgPackageWrapperInternal.PackageQty);
		}

		public void TestPackageSequence()
		{
			AssertEquals("PackageSequence", PkgPackageInternal.KP_Sequence, PkgPackageWrapperInternal.PackageSequence);
		}

		#endregion

		#region ZString Properties

		public void TestWeightUQ()
		{
			AssertEquals("WeightUQ", PkgPackageInternal.KP_WeightUQ, PkgPackageWrapperInternal.WeightUQ);
		}

		public void TestDimensionUQ()
		{
			AssertEquals("DimensionUQ", PkgPackageInternal.KP_DimensionUQ, PkgPackageWrapperInternal.DimensionUQ);
		}

		public void TestVolumeUQ()
		{
			AssertEquals("VolumeUQ", PkgPackageInternal.KP_VolumeUQ, PkgPackageWrapperInternal.VolumeUQ);
		}

		public void TestPackType()
		{
			AssertEquals("PackType", PkgPackageInternal.KP_F3_NKPackType, PkgPackageWrapperInternal.PackType);
		}

		public void TestMarksAndNumbers()
		{
			AssertEquals("MarksAndNumbers", PkgPackageInternal.KP_MarksAndNumbers, PkgPackageWrapperInternal.MarksAndNumbers);
		}

		public void TestSummary()
		{
			AssertEquals("Summary", PkgPackageInternal.KP_GoodsDescription, PkgPackageWrapperInternal.Summary);
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				PkgPackageWrapperInternal
			};
		}

		#region Implementation

		protected T PkgPackageInternal;
		protected TWrapper PkgPackageWrapperInternal => CreatePkgPackageWrapper(PkgPackageInternal);

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return CreatePkgPackageWrapper(PkgPackageInternal);
		}

		protected override void SetUp()
		{
			PkgPackageInternal = GetNewPkgPackage();
			base.SetUp();
		}

		protected virtual T GetNewPkgPackage()
		{
			return (T)Factory.New<PkgPackage>();
		}

		#endregion
	}
}
