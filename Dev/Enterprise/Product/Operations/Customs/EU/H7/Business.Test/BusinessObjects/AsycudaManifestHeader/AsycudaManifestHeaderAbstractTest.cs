using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestsSubclassesOf(typeof(AsycudaManifestHeader))]
	public abstract class AsycudaManifestHeaderAbstractTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>();

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString> { nameof(AsycudaManifestHeaderSchema.AMA_OA_Declarant) };
		}
	}
}
