using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceComponentIdentificationCollection : ActiveBusinessObjectCollection<ClientDeviceComponentIdentification>
	{
		public ClientDeviceComponentIdentificationCollection(ClientDeviceComponent parent)
			: base(parent.Factory, parent, null, DmgDeviceComponentIdentificationSchema.CDD_CDC_Component)
		{
		}

		public ClientDeviceComponentIdentificationCollection(BusinessObjectFactory factory, ZString identificationType, ZString identifier)
			: base(factory, MakeQueryForMatchingIdentifiers(identificationType, identifier))
		{
		}

		static ZQuery MakeQueryForMatchingIdentifiers(ZString identificationType, ZString identifier)
		{
			var matchingIdentifierQuery = new ZQuery(
				new ZQuery(DmgDeviceComponentIdentificationSchema.CDD_IdentificationType, SQLComparisonOperator.Equal, identificationType),
				JoinCondition.And,
				new ZQuery(DmgDeviceComponentIdentificationSchema.CDD_Identifier, SQLComparisonOperator.Equal, identifier));

			var deviceIsNotTemplateQuery = MakeQueryToFilterByDeviceTemplate(ZBool.False);

			return new ZQuery(matchingIdentifierQuery, JoinCondition.And, deviceIsNotTemplateQuery);
		}

		static ZDBOnlyQuery MakeQueryToFilterByDeviceTemplate(ZBool deviceIsTemplateValue)
		{
			var parentDeviceTemplateQuery = new ZDBOnlyQuery(typeof(ClientDeviceComponentIdentification));

			var queryToJoinComponentToDevice = new ZDBOnlySubQuery(typeof(ClientDeviceHeader), DmgDeviceComponentSchema.CDC_CDH_Device);
			queryToJoinComponentToDevice.AddToFilter(new ZQuery(DmgDeviceHeaderSchema.CDH_IsTemplate, SQLComparisonOperator.Equal, deviceIsTemplateValue));

			var queryToJoinIdentificationToComponent = new ZDBOnlySubQuery(typeof(ClientDeviceComponent), DmgDeviceComponentIdentificationSchema.CDD_CDC_Component);
			queryToJoinIdentificationToComponent.AddSubQuery(queryToJoinComponentToDevice, JoinCondition.And);

			parentDeviceTemplateQuery.AddSubQuery(queryToJoinIdentificationToComponent, JoinCondition.And);

			return parentDeviceTemplateQuery;
		}
	}
}

