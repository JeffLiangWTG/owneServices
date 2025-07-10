using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	public abstract class ActivityDataObjectWriter<T> : TopLevelDataObjectWriter<T, Activity>
		where T : BusinessObject
	{
		protected ActivityDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalActivity;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(T sourceBusinessObject)
		{
			var helper = ObjectFactory.Get<ICustomValuesHelper>();

			return helper.GetUserDefinedValues(sourceBusinessObject);
		}
	}
}
