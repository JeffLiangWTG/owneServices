using System;
using System.Globalization;

namespace Enterprise.ZArchitecture.Modules
{
	public class RegistrationInfo
	{
		internal RegistrationInfo(RegistrationIdentifier iD, Type registrationType, TableRegistrationInfo tableInfo = null) : this(iD, registrationType.Assembly.FullName, registrationType.FullName, tableInfo)
		{
		}

		internal RegistrationInfo(RegistrationIdentifier iD, Type registrationType, string countryCode, TableRegistrationInfo tableInfo = null) : this(iD, registrationType.Assembly.FullName, registrationType.FullName, countryCode, tableInfo)
		{
		}

		internal RegistrationInfo(RegistrationIdentifier iD, string assemblyName, string classFullName, TableRegistrationInfo tableInfo = null) : this(iD, assemblyName, classFullName, null, tableInfo)
		{
		}

		internal RegistrationInfo(RegistrationIdentifier iD, string assemblyName, string classFullName, string countryCode, TableRegistrationInfo tableInfo = null)
		{
			this.ID = iD;
			this.AssemblyName = assemblyName;
			this.ClassFullName = classFullName;
			this.CountryCode = countryCode ?? "";
			this.TableName = tableInfo == null ? "" : tableInfo.Name;
		}

		internal RegistrationIdentifier ID;
		internal string CountryCode;
		public readonly string TableName;

		public virtual bool IsClientOverride
		{
			get { return false; }
		}

		public virtual bool CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist
		{
			get { return false; }
		}

		public string TypePath
		{
			get { return ClassFullName + "," + AssemblyName; }
		}

		public override bool Equals(object obj)
		{
			RegistrationInfo rhs = obj as RegistrationInfo;
			return
				rhs != null &&
				rhs.ID.Equals(ID) &&
				rhs.CountryCode == CountryCode &&
				rhs.TableName == TableName;
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode() ^ CountryCode.GetHashCode() ^ TableName.GetHashCode();
		}

		#region Implementation

		protected string AssemblyName;
		protected string ClassFullName;

		#endregion

		public string GetDebuggerInfo() => string.Format(CultureInfo.CurrentCulture, @"ID: {0}
AssemblyName: {1}
ClassFullName: {2}
CountryCode: {3}
TableName: {4}", ID, AssemblyName, ClassFullName, CountryCode, TableName);

		#region Test
#if DEBUG

		public RegistrationIdentifier IDForTest => ID;

		public string CountryCodeForTest => CountryCode;

		public string AssemblyNameForTest => AssemblyName;

		public string ClassFullNameForTest => ClassFullName;

#endif
		#endregion

	}
}
