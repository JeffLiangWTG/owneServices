using System;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class InternetAddressRule : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Text = "Text";
			public const string Enabled = "Enabled";
		}

		#endregion

		#region Properties

		#region Text

		public ZString Text
		{
			get => text;
			set
			{
				SetNonPersistentPropertyValue(TextInfo, ref text, value);
				if (!IsValidationSuspended)
				{
					ValidateText();
				}
			}
		}

		public ZPropertyInfo TextInfo => GetZPropertyInfo(Schema.Text);

		ZString text;

		#endregion

		#region Enabled

		public ZBool Enabled
		{
			get => enabled;
			set => SetNonPersistentPropertyValue(EnabledInfo, ref enabled, value);
		}

		public ZPropertyInfo EnabledInfo => GetZPropertyInfo(Schema.Enabled);

		ZBool enabled;

		#endregion

		#region Address Type

		public IPAddressType AddressType
		{
			get
			{
				if (Text.IndexOf("/", StringComparison.Ordinal) != -1)
				{
					return IPAddressType.Subnet;
				}

				return Text.IndexOf("-", StringComparison.Ordinal) != -1 ? IPAddressType.Range : IPAddressType.Single;
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateText();
		}

		string InvalidIPAddressErrorMessage => Res.GetString("4785a744-efcc-43dc-a8d6-37635a41d4a8", "Please enter a valid IP address, range, or subnet in CIDR notation. e.g. 169.254.32.102, 192.168.0.0-192.168.1.255, 10.0.0.0/8. IPv6 is also supported.");

		protected void ValidateText()
		{
			TextInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TextInfo);

			switch (AddressType)
			{
				case IPAddressType.Subnet:
					if (!IPNetwork2.TryParse(Text, out var ipNetwork))
					{
						TextInfo.AddError(InvalidIPAddressErrorMessage);
					}
					else
					{
						var subnetInCIDRNotation = Text.Split('/');
						var hostAddress = IPAddress.Parse(subnetInCIDRNotation[0]);
						var hostAddressBigInteger = IPNetwork2.ToBigInteger(hostAddress);
						var networkAddressBigInteger = IPNetwork2.ToBigInteger(ipNetwork.Network);

						if (hostAddressBigInteger != networkAddressBigInteger && hostAddressBigInteger != networkAddressBigInteger + 1)
						{
							TextInfo.AddError(InvalidIPAddressErrorMessage);
						}
					}
					break;

				case IPAddressType.Range:
					var addresses = Text.Split('-');

					if (!IPAddress.TryParse(addresses[0], out var startAddress) || !IPAddress.TryParse(addresses[1], out var endAddress))
					{
						TextInfo.AddError(InvalidIPAddressErrorMessage);
					}
					else
					{
						var startAddressFamily = startAddress.AddressFamily;
						var endAddressFamily = endAddress.AddressFamily;

						if (startAddressFamily != endAddressFamily)
						{
							TextInfo.AddError(Res.GetString("44427687-9bb3-4eb0-aafe-1f266c771e46", "Please make sure that the start IP address and the end IP address of an IP range are of same protocol version."));
						}
						else
						{
							var startAddressBigInteger = IPNetwork2.ToBigInteger(startAddress);
							var endAddressBigInteger = IPNetwork2.ToBigInteger(endAddress);

							if (startAddressBigInteger > endAddressBigInteger)
							{
								TextInfo.AddError(Res.GetString("e714d50b-8b54-41fa-a267-4b2c1ee9712b", "Please make sure that the start IP address of an IP range is lower than the end IP address."));
							}
						}
					}
					break;

				default:
					if (!Text.IsEmpty && !IPAddress.TryParse(Text, out var singleAddress))
					{
						TextInfo.AddError(InvalidIPAddressErrorMessage);
					}
					break;
			}
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InternetAddressRule();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var rule = (InternetAddressRule)clone;
			rule.Text = Text;
			rule.Enabled = Enabled;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Text, Text);
			writer.WriteElementString(Schema.Enabled, Enabled.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Text = reader.ReadElementString(Schema.Text);
			Enabled = new ZBool(reader.ReadElementString(Schema.Enabled));
		}

		#endregion
	}

	public enum IPAddressType
	{
		Subnet,
		Range,
		Single
	}
}
