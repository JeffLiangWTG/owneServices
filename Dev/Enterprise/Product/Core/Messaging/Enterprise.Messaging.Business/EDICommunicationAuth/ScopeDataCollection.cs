using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business.EDICommunicationAuthInbound
{
	public class ScopeDataCollection : NonPersistentBusinessObjectCollection<ScopeData>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ScopeData();
		}

		readonly string _clientId;

		public ScopeDataCollection(string clientId)
		{
			_clientId = clientId;
		}

		public void LoadCollection()
		{
			if (!string.IsNullOrEmpty(_clientId))
			{
				Add(new ScopeData
				{
					Name = $"{_clientId}/.default"
				});
			}
		}
	}
}
