using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CodeAndDescription")]
	public class ContainerTypeWrapper : GenericWrapper
	{
		public ContainerTypeWrapper(RefContainer containerType, BusinessObjectFactory factory)
			: base(containerType, factory)
		{
			ContainerType = containerType ?? factory.GetNull<RefContainer>();
		}
		readonly RefContainer ContainerType;

		public ZString Code
		{
			get { return ContainerType.RC_Code; }
		}

		public ZString Description
		{
			get { return ContainerType.RC_DescriptionMultilingual; }
		}

		public ZString CodeAndDescription
		{
			get { return Code + (Code != Description ? (Code.IsEmpty || Description.IsEmpty ? "" : " - ") + Description : ""); }
		}

		public ZString ISOCode
		{
			get { return ContainerType.RC_ISOType; }
		}

		public ValueAndUnitWrapper TareWeight
		{
			get { return new ValueAndUnitWrapper(ContainerType.RC_TareWeight, Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory); }
		}
	}
}
