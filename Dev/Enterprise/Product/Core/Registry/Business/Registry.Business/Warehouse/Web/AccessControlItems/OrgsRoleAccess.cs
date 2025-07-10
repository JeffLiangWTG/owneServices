using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Web
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgsRoleAccess : RegistryBusinessObjectTemplate
	{
		public OrgsRoleAccess() { }

		public OrgsRoleAccess(ZString role, string[] captions)
		{
			this.role = role;
			LastUsedPropertyNum = captions.Length;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgsRoleAccess(role, new string[LastUsedPropertyNum]);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString("Role", Role);
			writer.WriteElementString("LastUsedPropertyNum", LastUsedPropertyNum.ToString());

			for (int i = 1; i <= LastUsedPropertyNum; i++)
			{
				string propertyName = string.Format((NoResString)"Property{0}", i);
				writer.WriteElementString(propertyName, this[propertyName].ToString());
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			role = reader.ReadElementString("Role");
			LastUsedPropertyNum = reader.ReadElementStringAsZInt("LastUsedPropertyNum");

			for (int i = 1; i <= LastUsedPropertyNum; i++)
			{
				string propertyName = string.Format((NoResString)"Property{0}", i);
				this[propertyName] = reader.ReadElementStringAsZBool(propertyName);
			}
		}

		#endregion

		public ZString Role
		{
			get { return role; }
		}
		ZString role;

		public ZPropertyInfo RoleInfo
		{
			get { return GetZPropertyInfo(nameof(Role)); }
		}

		#region Properties

		public int LastUsedPropertyNum { get; private set; }

		public ZBool Property1
		{
			get { return property1; }
			set
			{
				property1 = value;
				Property1Info.RefreshBinding();
			}
		}
		ZBool property1;

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		public ZBool Property2
		{
			get { return property2; }
			set
			{
				property2 = value;
				Property2Info.RefreshBinding();
			}
		}
		ZBool property2;

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		public ZBool Property3
		{
			get { return property3; }
			set
			{
				property3 = value;
				Property3Info.RefreshBinding();
			}
		}
		ZBool property3;

		public ZPropertyInfo Property3Info
		{
			get { return GetZPropertyInfo(nameof(Property3)); }
		}

		public ZBool Property4
		{
			get { return property4; }
			set
			{
				property4 = value;
				Property4Info.RefreshBinding();
			}
		}
		ZBool property4;

		public ZPropertyInfo Property4Info
		{
			get { return GetZPropertyInfo(nameof(Property4)); }
		}

		public ZBool Property5
		{
			get { return property5; }
			set
			{
				property5 = value;
				Property5Info.RefreshBinding();
			}
		}
		ZBool property5;

		public ZPropertyInfo Property5Info
		{
			get { return GetZPropertyInfo(nameof(Property5)); }
		}

		public ZBool Property6
		{
			get { return property6; }
			set
			{
				property6 = value;
				Property6Info.RefreshBinding();
			}
		}
		ZBool property6;

		public ZPropertyInfo Property6Info
		{
			get { return GetZPropertyInfo(nameof(Property6)); }
		}

		public ZBool Property7
		{
			get { return property7; }
			set
			{
				property7 = value;
				Property7Info.RefreshBinding();
			}
		}
		ZBool property7;

		public ZPropertyInfo Property7Info
		{
			get { return GetZPropertyInfo(nameof(Property7)); }
		}

		public ZBool Property8
		{
			get { return property8; }
			set
			{
				property8 = value;
				Property8Info.RefreshBinding();
			}
		}
		ZBool property8;

		public ZPropertyInfo Property8Info
		{
			get { return GetZPropertyInfo(nameof(Property8)); }
		}

		public ZBool Property9
		{
			get { return property9; }
			set
			{
				property9 = value;
				Property9Info.RefreshBinding();
			}
		}
		ZBool property9;

		public ZPropertyInfo Property9Info
		{
			get { return GetZPropertyInfo(nameof(Property9)); }
		}

		public ZBool Property10
		{
			get { return property10; }
			set
			{
				property10 = value;
				Property10Info.RefreshBinding();
			}
		}
		ZBool property10;

		public ZPropertyInfo Property10Info
		{
			get { return GetZPropertyInfo(nameof(Property10)); }
		}

		public ZBool Property11
		{
			get { return property11; }
			set
			{
				property11 = value;
				Property11Info.RefreshBinding();
			}
		}
		ZBool property11;

		public ZPropertyInfo Property11Info
		{
			get { return GetZPropertyInfo(nameof(Property11)); }
		}

		public ZBool Property12
		{
			get { return property12; }
			set
			{
				property12 = value;
				Property12Info.RefreshBinding();
			}
		}
		ZBool property12;

		public ZPropertyInfo Property12Info
		{
			get { return GetZPropertyInfo(nameof(Property12)); }
		}

		public ZBool Property13
		{
			get { return property13; }
			set
			{
				property13 = value;
				Property13Info.RefreshBinding();
			}
		}
		ZBool property13;

		public ZPropertyInfo Property13Info
		{
			get { return GetZPropertyInfo(nameof(Property13)); }
		}

		#endregion
	}
}
