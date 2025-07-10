using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class ParameterCollection : NonPersistentBusinessObjectCollection<Parameter>
	{
		public ParameterCollection(string eventCode)
		{
			this.eventCode = eventCode;
		}

		readonly string eventCode;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Parameter(eventCode);
		}
	}
}
