using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class PkgUnitXmlCodeMappingsIncludingReferenceFiles : PkgUnitXmlCodeMappings
	{
		public PkgUnitXmlCodeMappingsIncludingReferenceFiles(IFactory factory)
		{
			packTypeMappings = factory
				.Load<RefPackType>(new ZQuery())
				.Select(packType => new Mapping(packType.F3_Code, packType.F3_Code))
				.ToImmutableArray();
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			var result = base.GetMappings().ToList();
			foreach (var packType in packTypeMappings)
			{
				if (!ContainsEnterpriseCode(result, packType.EnterpriseCode))
				{
					result.Add(packType);
				}
			}
			return result;
		}

		readonly ImmutableArray<Mapping> packTypeMappings;
	}
}
