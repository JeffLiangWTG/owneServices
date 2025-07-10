using System;
using System.Linq;
using System.Reflection;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	#region SuppressResourceStringsCheckRegion

	public class MethodDescription : MemberDescription
	{
#if DEBUG
		/// <summary>
		/// Only for tests
		/// </summary>
		public MethodDescription(MethodInfo method, MemberDescription parentMemberDescription)
			: this(method, parentMemberDescription, "", MacroTagTypes.Document, new DocDataReflectorFilter())
		{
		}
#endif

		public MethodDescription(
			MethodInfo method,
			MemberDescription parentMemberDescription,
			string helpText,
			MacroTagTypes macroTagType,
			IDataReflectorFilter filter)
			: base(parentMemberDescription, helpText, macroTagType, filter)
		{
			if (method == null)
			{
				throw new ArgumentNullException("MethodInfo method");
			}

			this.Method = method;
		}

		public readonly MethodInfo Method;

		public override bool CanHaveChildMembers()
		{
			return Filter.IsCollection(Method.ReturnType) || Filter.IsRelatedObject(Method.ReturnType);
		}

		public override (Type ChildType, Type PossibleCollectionType) GetChildTypes()
		{
			var methodReturnType = Method.ReturnType;

			if (Filter.IsCollection(methodReturnType))
			{
				return (GetCollectionChildType(methodReturnType), null);
			}

			if (Filter.IsRelatedObject(methodReturnType))
			{
				return (methodReturnType, null);
			}

			throw new InvalidOperationException("You should never call GetChildType(MethodInfo method) unless CanHaveChildMembers(MethodInfo method) is true.");
		}

		public override string GetFormattedTextLabel()
		{
			return FormattableString.Invariant($"{MethodNameWithParameters} ({Method.ReturnType.Name})");
		}

		public override string GetFullPath()
		{
			if (ParentMemberDescription != null)
			{
				return ParentMemberDescription.GetFullPath() + "." + MethodNameWithParameters;
			}
			return MethodNameWithParameters;
		}

		internal string MethodNameWithParameters
		{
			get { return Method.Name + "(" + string.Join(",", Method.GetParameters().Select(p => "{" + p.Name + "}").ToArray()) + ")"; }
		}
	}

	#endregion
}
