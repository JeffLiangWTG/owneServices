using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture;

namespace Enterprise.MailManager.MailFilters
{
	public class MailFilterLocator : IMailFilterProvider
	{
		readonly IMailFilterProvider[] filters;

		public MailFilterLocator()
			: this(true)
		{
		}

		public MailFilterLocator(bool retrieveForAllClients)
			: this(AssemblyMetaDataReader.GetAttributes<MailSubscriberAttribute>(retrieveForAllClients).Append<IMailFilterProvider>(new MailFilterProvider()))
		{
		}

		public MailFilterLocator(IEnumerable<IMailFilterProvider> filters)
			=> this.filters = filters.ToArray();

		public IEnumerable<IMailFilter> GetFilters()
			=> GetFiltersIncludingDisabled().Where(x => x.IsEnabled);

		public IEnumerable<IMailFilter> GetFiltersIncludingDisabled()
			=> filters.SelectMany(f => f.GetFilters());

		public bool TryGetFilter(string code, out IMailFilter filter)
		{
			foreach (var provider in filters)
			{
				if (provider.TryGetFilter(code, out filter))
				{
					return true;
				}
			}

			filter = null;
			return false;
		}
	}
}
