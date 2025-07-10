using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class InternetAddressRuleset : RegistryBusinessObjectCollectionTemplate
	{
		public new InternetAddressRule this[int i] => (InternetAddressRule)Elements[i];

		public new InternetAddressRule AddNew()
		{
			return (InternetAddressRule)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InternetAddressRule();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel,
			BusinessObjectFactory factory)
		{
			return new InternetAddressRuleset();
		}

		public IEnumerable<InternetAddressRule> GetEnabledRules()
		{
			return this.Cast<InternetAddressRule>().Where(item => item.Enabled);
		}

		public bool Contains(string ip)
		{
			if (!IPAddress.TryParse(ip, out var address))
			{
				return false;
			}

			foreach (var rule in GetEnabledRules().ToArray())
			{
				switch (rule.AddressType)
				{
					case IPAddressType.Subnet:
						var ipnetwork = IPNetwork2.Parse(rule.Text);
						if (ipnetwork.Contains(address))
						{
							return true;
						}
						break;

					case IPAddressType.Range:
						var range = rule.Text.Split('-');
						var startAddress = IPAddress.Parse(range[0]);
						var endAddress = IPAddress.Parse(range[1]);

						var startAddressBigInteger = IPNetwork2.ToBigInteger(startAddress);
						var endAddressBigInteger = IPNetwork2.ToBigInteger(endAddress);
						var addressBigInteger = IPNetwork2.ToBigInteger(address);

						if (addressBigInteger >= startAddressBigInteger && addressBigInteger <= endAddressBigInteger)
						{
							return true;
						}
						break;

					case IPAddressType.Single:
						var singleAddress = IPAddress.Parse(rule.Text);
						var singleAddressBigInteger = IPNetwork2.ToBigInteger(singleAddress);
						var ipAddressBigInteger = IPNetwork2.ToBigInteger(address);

						if (singleAddressBigInteger == ipAddressBigInteger)
						{
							return true;
						}
						break;
				}
			}

			return false;
		}
	}
}
