using System;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Provides a list of IBindingMemberForCompileTimeCheckS for use by the KBindingSource to
	/// generate compile-time property navigation checks.
	/// </summary>
	public interface IBindingMemberForCompileTimeCheckProvider
	{
		CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember);
	}
}
