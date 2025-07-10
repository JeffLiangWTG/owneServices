using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class ContainerValueObjectHelper
	{
		public ContainerValueObjectHelper(IValueObjectImportContext context)
		{
			this.Context = context;
		}

		readonly IValueObjectImportContext Context;

		public void ImportContainerType(ZPropertyInfo containerPropertyInfo, Xsd.ContainerType containerType)
		{
			if (containerType != null && containerType.IsSpecified)
			{
				if (!containerType.ContainerCode.IsEmpty)
				{
					Context.SetPropertyInfoValue(containerPropertyInfo, containerType.ContainerCode, ForeignKeyType.ContainerCodeNK);
				}

				if (containerPropertyInfo.Value.IsEmpty && !containerType.ISOCode.IsEmpty)
				{
					RefContainer[] matchingContainers = Context.Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, containerType.ISOCode));
					if (matchingContainers.Length == 0)
					{
						Context.Notify(new ErrorNotification(ErrorType.MissingPKFromNK, Res.GetString("ebb1a75b-e4ce-4f43-9496-6e5f0ffb0658", "No Container type found for ISO Code {0}", containerType.ISOCode)));
					}
					else if (matchingContainers.Length == 1)
					{
						Context.SetPropertyInfoValue(containerPropertyInfo, matchingContainers[0].RC_Code, ForeignKeyType.ContainerCodeNK);
					}
					else
					{
						Context.Notify(new ErrorNotification(ErrorType.MoreThan1NKMatch, Res.GetString("1b71c9b5-f8e5-463a-b84b-dcae7b90f8fd", "Multiple container types found for ISO Code {0}", containerType.ISOCode)));
					}
				}
			}
		}
	}
}
