using System;
using System.ComponentModel;
using System.Diagnostics;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Contains information that enables a Binding to resolve a data binding to either the property
	/// of an object or the property of the current object in a list of objects.
	/// </summary>
	[DebuggerDisplay("{BindingMember}")]
	public struct KBindingMemberInfo
	{
		public KBindingMemberInfo(string bindingPath, PropertyDescriptor bindingField)
			: this(bindingPath, bindingField.Name)
		{
			Argument.NotNull(bindingField, nameof(bindingField));
		}

		// For structs, need to call the parameterless constructor to initialise all fields so the fields can be assigned
		// http://stackoverflow.com/a/5275151/1007496
		public KBindingMemberInfo(string bindingPath, string bindingField)
			: this()
		{
			bindingPath = ParseBindingPathOrField(bindingPath);
			bindingField = ParseBindingPathOrField(bindingField);

			this.bindingPath = bindingPath;
			this.bindingField = bindingField;
			if (bindingPath.Length == 0)
			{
				this.bindingMember = bindingField;
			}
			else if (bindingField.Length == 0)
			{
				this.bindingMember = bindingPath;
			}
			else
			{
				this.bindingMember = string.Concat(bindingPath, ".", bindingField);
			}
		}

		public KBindingMemberInfo(string bindingMember)
			: this()
		{
			bindingMember = (bindingMember == null) ? string.Empty : bindingMember.Trim();
			bindingMember = (bindingMember == ".") ? string.Empty : bindingMember;
			this.bindingMember = bindingMember;
			int i = bindingMember.LastIndexOf('.');
			if (i != -1)
			{
				this.bindingPath = bindingMember.Substring(0, i);
				this.bindingField = bindingMember.Substring(i + 1);
			}
			else
			{
				this.bindingPath = "";
				this.bindingField = bindingMember;
			}
		}

		static string ParseBindingPathOrField(string bind)
		{
			var result = "";
			if (bind != null)
			{
				var trim = bind.Trim();
				if (trim != ".")
				{
					result = trim;
				}
			}
			return result;
		}

		public string BindingPath
		{
			get { return bindingPath; }
		}
		readonly string bindingPath;

		public string BindingField
		{
			get { return bindingField; }
		}
		readonly string bindingField;

		public string BindingMember
		{
			get { return bindingMember; }
		}
		readonly string bindingMember;

		public static implicit operator string(KBindingMemberInfo member)
		{
			return member.BindingMember;
		}

		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is KBindingMemberInfo)
			{
				var bindingMemberInfo = (KBindingMemberInfo)obj;
				result = string.Compare(BindingMember, bindingMemberInfo.BindingMember, StringComparison.OrdinalIgnoreCase) == 0;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return BindingMember.GetHashCode();
		}

		public static bool operator ==(KBindingMemberInfo lhs, KBindingMemberInfo rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(KBindingMemberInfo lhs, KBindingMemberInfo rhs)
		{
			return !(lhs == rhs);
		}
	}
}
