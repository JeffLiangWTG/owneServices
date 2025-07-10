using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class LegTypeCollection : RegistryBusinessObjectCollection
	{
		public LegTypeCollection()
		{
		}

		public LegTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new LegType this[int i]
		{
			get { return (LegType)Elements[i]; }
		}

		public new LegType AddNew()
		{
			return (LegType)base.AddNew();
		}

		public new LegType FindByCode(string code)
		{
			return (LegType)base.FindByCode(code);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LegTypeCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LegType(CurrentFactory);
		}

		#region Lists

		#region LegTypes_List

		public CodeDescriptionPairList LegTypes_List
		{
			get
			{
				if (fLegTypes_List == null)
				{
					fLegTypes_List = new CodeDescriptionPairList();
					fLegTypes_List.AddRange(this);
				}
				return fLegTypes_List;
			}
		}
		CodeDescriptionPairList fLegTypes_List;

		#endregion

		#region LegTypesForImport_List

		public CodeDescriptionPairList LegTypesForImport_List
		{
			get
			{
				if (fLegTypesForImport_List == null)
				{
					fLegTypesForImport_List = new CodeDescriptionPairList();
					foreach (LegType registryLegType in this)
					{
						if (registryLegType.MovementType == Constants.CartageDirection.Destination)
						{
							fLegTypesForImport_List.AddPair(registryLegType.Code, registryLegType.Description);
						}
					}
				}
				return fLegTypesForImport_List;
			}
		}
		CodeDescriptionPairList fLegTypesForImport_List;

		#endregion

		#region LegTypesForExport_List

		public CodeDescriptionPairList LegTypesForExport_List
		{
			get
			{
				if (fLegTypesForExport_List == null)
				{
					fLegTypesForExport_List = new CodeDescriptionPairList();
					foreach (LegType registryLegType in this)
					{
						if (registryLegType.MovementType == Constants.CartageDirection.Origin)
						{
							fLegTypesForExport_List.AddPair(registryLegType.Code, registryLegType.Description);
						}
					}
				}
				return fLegTypesForExport_List;
			}
		}
		CodeDescriptionPairList fLegTypesForExport_List;

		#endregion

		#region LegTypes_ListWith

		public CodeDescriptionPairList LegTypes_ListWith(bool isImport, bool isContainerised)
		{
			string movementType = isImport ? Constants.CartageDirection.Destination : Constants.CartageDirection.Origin;
			string containerType = isContainerised ? Constants.ContainerModes.Containerised : Constants.ContainerModes.Loose;

			CodeDescriptionPairList result = new CodeDescriptionPairList();

			foreach (LegType registryLegType in this)
			{
				if (registryLegType.MovementType == movementType && registryLegType.Containerised == containerType)
				{
					result.AddPair(registryLegType.Code, registryLegType.Description);
				}
			}
			return result;
		}

		#endregion

		#endregion
	}
}
