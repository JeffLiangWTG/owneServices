using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	public class DocContainerAndPackageInfoCollection : DocumentWrapperCollection
	{
		public DocContainerAndPackageInfoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocContainerAndPackageInfoCollection(JobDeclaration collectionSource)
			: base(collectionSource.Factory)
		{
			Declaration = collectionSource;
		}

		protected readonly JobDeclaration Declaration;

		public new DocContainerAndPackageInfo this[int index]
		{
			get { return (DocContainerAndPackageInfo)base[index]; }
		}

		#region Load
		public override void Load()
		{
			RemoveAll();
			if (Declaration.IsECIWriteoff)
			{
				LoadECIWriteOffDocContainerAndPackages();
			}
			else
			{
				LoadFormalEntryDocContainerAndPackages();
			}
		}
		#endregion

		#region LoadFormalEntryDocContainerAndPackages
		void LoadFormalEntryDocContainerAndPackages()
		{
			foreach (Bill houseBill in Declaration.LowestBills)
			{
				foreach (PackingGroup packingGroup in houseBill.PackingGroups)
				{
					ZInt numberOfPackages = packingGroup.TotalPackageCount();
					ZString billNumber = houseBill.CU_HouseBill;

					ZString containerNumber = "";
					ZString containerStatus = "";
					if (packingGroup.Container != null)
					{
						containerNumber = packingGroup.Container.CO_ContainerNumber;
						containerStatus = packingGroup.Container.CO_FCL_LCL_AIR;
					}

					bool foundPackages = false;
					ZStringBuilder packagesAndTypes = new ZStringBuilder();
					foreach (Package package in packingGroup.Packages)
					{
						if (package.CW_PackQty > 0)
						{
							if (!packagesAndTypes.IsEmpty)
							{
								packagesAndTypes.Append(", ");
							}

							packagesAndTypes.Append(package.CW_PackQty.ToString());
							packagesAndTypes.Append(package.CW_PackType.IsEmpty ? "" : " " + package.CW_PackType);
							foundPackages = true;
						}
					}

					if (foundPackages)
					{
						Add(new DocContainerAndPackageInfo(Factory, billNumber, Declaration.IsAir ? "HWB" : "HBL", containerNumber, containerStatus, packagesAndTypes.ToString()));
					}
					else if (!containerNumber.IsEmpty)
					{
						Add(new DocContainerAndPackageInfo(Factory, billNumber, Declaration.IsAir ? "HWB" : "HBL", containerNumber, containerStatus, "0"));
					}
				}
			}
		}

		#endregion

		#region LoadECIWriteOffDocContainerAndPackages
		void LoadECIWriteOffDocContainerAndPackages()
		{
			ZString billNumber = Declaration.JE_HouseBill;
			ZString containerNumber = "";
			ZString containerStatus = "";
			ZInt numberOfPackages = Declaration.JE_TotalNoOfPacks;

			if (Declaration.CusContainers.Count == 1 && Declaration.IsSea)
			{
				CusContainer container = Declaration.CusContainers[0];
				containerNumber = container.CO_ContainerNumber;
				containerStatus = container.CO_FCL_LCL_AIR;
			}

			Add(new DocContainerAndPackageInfo(Factory, billNumber, Declaration.IsAir ? "HWB" : "HBL", containerNumber, containerStatus, numberOfPackages + " PCS"));

			if (Declaration.CusContainers.Count > 1 && Declaration.IsSea)
			{
				foreach (CusContainer container in Declaration.CusContainers)
				{
					Add(new DocContainerAndPackageInfo(Factory, "", "", container.CO_ContainerNumber, container.CO_FCL_LCL_AIR, "0"));
				}
			}
		}

		#endregion
	}
}
