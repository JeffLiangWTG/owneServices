using System;
using System.Dynamic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Macros;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Macro
{
	[CodeAlive("It is supposed to be used in future workitems.")]
	public class DynamicMacroObject : DynamicObject
	{
		readonly object[] nested;
		readonly IMacroEvaluationContext context;
		IMacroScope parentScope;

		public DynamicMacroObject(IMacroEvaluationContext macroEvaluationContext, params object[] orderedObjects)
		{
			this.nested = orderedObjects ?? throw new ArgumentNullException(nameof(orderedObjects));
			this.context = macroEvaluationContext;
		}

		public void SetParentScope(IMacroScope scope)
		{
			parentScope = scope;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = null;
			var macro = binder.Name.With(context).CreateExpression();
			foreach (var item in nested)
			{
				using var scope = new MacroScope(parentScope, item);
				result = macro.Evaluate(scope);

				if (result != null)
				{
					return true;
				}
			}

			return false;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			foreach (var item in nested)
			{
				var macroMetadata = context.FindMetaData(binder.Name);
				if (macroMetadata != null)
				{
					var handlerArgs = args;
					var handler = macroMetadata.FindBestMatchingHandler(handlerArgs);
					if (handler == null)
					{
						handlerArgs = new[] { item }.Concat(args).ToArray();
						handler = macroMetadata.FindBestMatchingHandler(handlerArgs);
					}

					if (handler != null)
					{
						try
						{
							handlerArgs = handler.NeedsScope ? new[] { parentScope }.Concat(handlerArgs).ToArray() : handlerArgs;
							result = handler.Handler.DynamicInvoke(handlerArgs);
							return true;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
						}
					}
				}
			}

			result = null;
			return false;
		}
	}
}
