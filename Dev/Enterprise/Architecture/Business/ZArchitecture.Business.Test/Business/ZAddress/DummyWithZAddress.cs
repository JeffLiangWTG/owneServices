using System;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyWithZAddress : DummyEnterpriseBusinessObject
	{
		public DummyWithZAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : DummyBaseBusinessObject.Schema
		{
			public const string Z0_GuidWrapped = "Z0_GuidWrapped";
		}

		#region Addy

		public ZAddress Addy
		{
			get
			{
				if (fAddy == null)
				{
					fAddy = new ZAddress(Z0_GuidInfo);
					fAddy.OnOrgChanged += new EventHandler(fAddy_OnOrgChanged);
				}
				return fAddy;
			}
		}

		public ZAddress Z0_Guid_ZAddress
		{
			get { return Addy; }
		}

		[List("Organisations")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member Z0_Guid_ReadOnly")]
		[ReadOnlyMember("Z0_Guid_ReadOnly")]
		public override ZGuid Z0_Guid
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Z0_Guid; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Z0_Guid = value; }
		}

		void fAddy_OnOrgChanged(object sender, EventArgs e)
		{
			IsAddyOnOrgChangedFired = true;
		}

		public bool IsAddyOnOrgChangedFired;
		ZAddress fAddy;

		public ZGuid Z0_GuidTest;
		public ZGuid Z0_GuidWrapped
		{
			get { return Z0_Guid.IsEmpty ? Z0_GuidTest : Z0_Guid; }
			set { Z0_Guid = (Z0_GuidTest == value) ? ZGuid.Empty : value; }
		}

		public ZPropertyInfo Z0_GuidWrappedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Z0_GuidWrapped, x => Z0_GuidInfo); }
		}

		#endregion

		#region Dummies (with Addys)

		public DummyWithZAddressCollection Dummies
		{
			get
			{
				if (fDummies == null)
				{
					fDummies = new DummyWithZAddressCollection(Factory);
				}
				return fDummies;
			}
		}

		DummyWithZAddressCollection fDummies;

		#endregion

		#region Organisations

		public BusinessObjectCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IOrgHeaderCollection>(), new object[] { Factory });
				}
				return fOrganisations;
			}
		}
		BusinessObjectCollection fOrganisations;

		#endregion
	}
}
