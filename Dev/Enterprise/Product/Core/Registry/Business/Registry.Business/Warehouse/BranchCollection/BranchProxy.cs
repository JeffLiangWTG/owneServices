using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BranchProxy : RegistryProxyBusinessObject
	{
		#region Properties

		#region BranchCode

		[ResourceStringData("Branch|BranchCode", Caption = "Code")]
		public ZString BranchCode
		{
			get
			{
				var branch = GetProxy<IGlbBranch>();
				return branch != null ? branch.GB_Code : InvalidBranchMessage;
			}
		}

		static ZString InvalidBranchMessage
		{
			get { return Res.GetString("cda5e571-1437-4e59-8f3c-78a8b0908b78", "Invalid Branch"); }
		}

		#endregion

		#region Name

		[ResourceStringData("Branch|Name", Caption = "Name")]
		public ZString Name
		{
			get
			{
				var branch = GetProxy<IGlbBranch>();
				return branch != null ? branch.GB_BranchName : InvalidBranchMessage;
			}
		}

		#endregion

		#endregion

		#region Clone

		protected override RegistryProxyBusinessObject GetNew(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchProxy();
		}

		#endregion

		#region Delete

		protected override Type ParentCollectionType
		{
			get { return typeof(BranchProxyCollection); }
		}

		#endregion
	}
}
