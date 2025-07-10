using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Common.Collections;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Holds a string that contains the bind to member of a control so we can create CodeDom code
	/// that creates a new data source type and ensures we can bind to the target. The purpose of
	/// this is to check the validity of the BindingMember at compile time.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializer(typeof(CompileTimeCheckBindingMemberCodeDomSerializer), typeof(CodeDomSerializer))]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[WTG.StaticAnalysis.Annotation.Immutable]
	[Serializable]
	public class CompileTimeCheckBindingMember
	{
		/// <summary>
		/// Called by the control binder to give the CodeDom generator enough information to
		/// create the compile-time checking generated code.
		/// </summary>
		public CompileTimeCheckBindingMember(Type dataSourceType, Type controlPropertyType, string bindingMember)
		{
			this.dataSourceType = dataSourceType;
			this.controlPropertyType = controlPropertyType;
			this.bindingMember = bindingMember;
		}

		public CompileTimeCheckBindingMember(string error)
		{
			Argument.NotNull(error, nameof(error));
			isError = true;
			errorMessage = error;
		}

		internal CompileTimeCheckBindingMember(bool isEmpty)
		{
			this.isEmpty = isEmpty;
		}

		/// <summary>
		/// Used by the CodeDom generated code to pass in whatever is wants.
		/// </summary>
		public CompileTimeCheckBindingMember(object garbageThatIsNeverUsed)
		{
		}

		public static CompileTimeCheckBindingMember Empty
		{
			get
			{
				return empty;
			}
		}
		readonly static CompileTimeCheckBindingMember empty = new CompileTimeCheckBindingMember(true);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Name search string")]
		internal static bool IsList(Type type)
		{
			var result = false;
			if (type != null)
			{
				result = (typeof(IList).IsAssignableFrom(type) && (ListUtil.GetListElementType(type) != null || type == typeof(IList))) || (type.Name.StartsWith("IList`1", StringComparison.OrdinalIgnoreCase));
			}
			return result;
		}

		/// <summary>
		/// Utility method for statically navigating a path of properties. This should only be used for
		/// compile time checks as static navigation may not be precise considering people can sub-class
		/// PropertyDescriptors. Null is returned if there is an error navigating the properties.
		/// </summary>
		public PropertyDescriptor[] NavigateProperties(Type dataSourceType, string dataMember)
		{
			ArrayList result = null;
			var currentType = dataSourceType;
			if (currentType != null)
			{
				var members = new ArrayList();
				var nextDataMember = new KBindingMemberInfo(dataMember);
				while (!string.IsNullOrEmpty(nextDataMember.BindingMember))
				{
					members.Insert(0, nextDataMember.BindingField);
					nextDataMember = new KBindingMemberInfo(nextDataMember.BindingPath);
				}

				result = new ArrayList();
				foreach (string member in members)
				{
					if (IsList(currentType))
					{
						currentType = ListUtil.GetListElementType(currentType);
					}

					PropertyDescriptor property = null;
					if (currentType != null)
					{
						var properties = GetProperties(currentType);
						property = properties != null ? properties[member] : null;
					}

					if (property == null)
					{
						result = null;
						break;
					}
					else
					{
						currentType = property.PropertyType;
						result.Add(property);
					}
				}
			}
			return (result == null) ? null : (PropertyDescriptor[])result.ToArray(typeof(PropertyDescriptor));
		}

		public virtual PropertyDescriptorCollection GetProperties(Type type)
		{
			Argument.NotNull(type, nameof(type));
			return PropertyDescriptorCollectionWithWrappingProperties.FromType(type);
		}

		[Conditional("dont_run_this_EVER")]
		public static void Check(object neverUsed)
		{
		}

		/// <summary>
		/// Get the type of the data source being bound. Null to generate code that notifies the
		/// user of no attribute being applied.
		/// </summary>
		public Type DataSourceType
		{
			get { return dataSourceType; }
		}
		readonly Type dataSourceType;

		/// <summary>
		/// Get the Type of the value the control binds to.
		/// </summary>
		public Type ControlPropertyType
		{
			get { return controlPropertyType; }
		}
		readonly Type controlPropertyType;

		/// <summary>
		/// Get the actual BindingMember for the serializer.
		/// </summary>
		public string BindingMember
		{
			get { return bindingMember; }
		}
		readonly string bindingMember;

		/// <summary>
		/// Get whether the member is empty (no compile time check).
		/// </summary>
		public bool IsEmpty
		{
			get { return isEmpty; }
		}
		readonly bool isEmpty;

		/// <summary>
		/// Get whether this binding an error that should always trip up the compiler.
		/// </summary>
		public bool IsError
		{
			get { return isError; }
		}
		readonly bool isError;

		/// <summary>
		/// Get the error message (if IsError is true).
		/// </summary>
		public string ErrorMessage
		{
			get { return errorMessage; }
		}
		readonly string errorMessage = "";
	}
}
