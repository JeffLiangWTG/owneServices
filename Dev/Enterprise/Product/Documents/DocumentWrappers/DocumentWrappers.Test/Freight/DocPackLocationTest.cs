using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocPackLocation))]
	sealed class DocPackLocationTest : DocumentWrapperTestCase
	{
		public void TestProperties()
		{
			location.JQ_DeliveredDate = today;
			location.JQ_NoPackages = 100;
			location.JQ_VolumeUV = Core.Constants.Volume.CubicMetres;
			location.JQ_Volume = 10.2M;
			location.JQ_WeightUW = Core.Constants.Weight.Kilograms;
			location.JQ_Weight = 20.3M;

			DocPackLocation wrapper = DocPackLocation.New(location, Factory);
			AssertEquals(today, wrapper.DeliveredDate);
			AssertEquals(100, wrapper.NoPackages);
			AssertEquals(10.2M, wrapper.Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, wrapper.VolumeUV);
			AssertEquals(20.3M, wrapper.Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, wrapper.WeightUW);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocPackLocation.New(location, Factory)
				};
		}

		#region Implementation
		PackLocation location;
		ZDateTime today;

		protected override void SetUp()
		{
			today = ZDateTime.Now;
			location = Factory.New<PackLocation>();

			base.SetUp();
		}
		#endregion
	}
}
