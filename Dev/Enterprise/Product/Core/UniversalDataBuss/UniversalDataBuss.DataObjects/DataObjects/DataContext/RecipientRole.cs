using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class RecipientRole : IRecipientRoleDataObject
	{
		public static RecipientRole New(RecipientRoleDetail recipientRole)
		{
			var result = new RecipientRole();
			var code = recipientRole.Type;
			result.Code = code;
			var recipientTypeList = ObjectFactory.New<IAllPossibleRecipientTypesGetter>().GetRecipientList();
			result.Description = recipientTypeList.GetDescriptionFromCode(code.ToString());
			result.ServiceCode = recipientRole.ServiceCode;
			if (result.ServiceCode.HasValue)
			{
				result.ServiceDescription = result.ServiceCode.Value.GetDescription();
			}
			return result;
		}

		[Mandatory]
		public RecipientRoleType? Code { get; set; }
		[MaxLength(50)]
		public ZString? Description { get; set; }
		public ServiceCodeType? ServiceCode { get; set; }
		[MaxLength(50)]
		public ZString? ServiceDescription { get; set; }
	}
}
