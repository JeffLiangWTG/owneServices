using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	public class MiscellaneousDescriptionsCollection : DocumentWrapperCollection
	{
		public MiscellaneousDescriptionsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MiscellaneousDescriptionsCollection(DocDeclaration collectionSource)
			: base(collectionSource.Factory)
		{
			Declaration = collectionSource;
		}

		public new MiscellaneousDescription this[int index]
		{
			get { return (MiscellaneousDescription)base[index]; }
		}

		public override void Load()
		{
			ArrayList all = new ArrayList();
			all.Add(Declaration.GoodsDescription);
			foreach (DocContainerAndPackageInfo info in Declaration.DocContainerAndPackageInfos)
			{
				all.Add(info.PackagesAndType);
			}
			all.AddRange(Declaration.MarksAndNumberArray);

			int collectionLength = all.Count / NumberOfRowsPerElement + 1;
			for (int i = 0; i < collectionLength; i++)
			{
				int j = 0;
				ZStringBuilder result = new ZStringBuilder();
				for (j = 0; j < NumberOfRowsPerElement; j++)
				{
					if (all.Count > NumberOfRowsPerElement * i + j)
					{
						result.Append(all[NumberOfRowsPerElement * i + j].ToString().Replace("\r", "").Replace("\n", ""));
					}
				}
				this.Add(new MiscellaneousDescription(Factory, result.ToStringWithNewLineBetweenAppends()));
			}
		}

		const int NumberOfRowsPerElement = 25;
		protected readonly DocDeclaration Declaration;
	}
}
