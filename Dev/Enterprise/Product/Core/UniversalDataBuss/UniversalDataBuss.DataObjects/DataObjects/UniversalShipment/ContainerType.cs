using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class ContainerType : ICodeDescriptionDataObject
	{
		public static ContainerType New(IRefContainer typeBO)
		{
			ContainerType result = null;
			if (typeBO != null)
			{
				result = new ContainerType()
				{
					Code = typeBO.RC_Code,
					Description = typeBO.RC_Description,
					ISOCode = typeBO.RC_ISOType
				};
				var type = typeBO.RC_ContainerType;
				if (!type.IsEmpty)
				{
					CodeDescriptionPairList list = null;
					var bizObj = typeBO as IBusiness;
					if (bizObj != null)
					{
						list = bizObj.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.ContainerType);
					}
					list = list ?? new CodeDescriptionPairList(OLookUpEditType.ContainerType);
					result.Category = new ContainerTypeCategory()
					{
						Code = type,
						Description = list.GetDescriptionFromCode(type)
					};
				}
			}
			return result;
		}

		[MaxLength(10), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.ContainerType)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
		[MaxLength(4)]
		public ZString? ISOCode { get; set; }
		public ContainerTypeCategory Category { get; set; }
	}
}
