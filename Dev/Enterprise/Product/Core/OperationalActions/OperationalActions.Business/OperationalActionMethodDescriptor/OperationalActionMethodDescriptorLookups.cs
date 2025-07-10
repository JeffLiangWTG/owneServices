using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionMethodDescriptorLookups : ZLookups
	{
		public OperationalActionMethodDescriptorLookups(OperationalActionMethodDescriptor parent)
			: base(parent) { }

		public CodeDescriptionPairList MethodGroups
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				OperationalActionSupporter actionSupporter = Parent.Context.Supporter;

				if (actionSupporter != null)
				{
					foreach (ActionMethodProviderID id in actionSupporter.Methods.GetAllIds())
					{
						result.Add(id);
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList MethodNames
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				OperationalActionSupporter actionSupporter = Parent.Context.Supporter;

				if (actionSupporter != null)
				{
					foreach (OperationalActionMethod method in actionSupporter.Methods.GetMethods(Parent.MethodGroup))
					{
						result.AddPair(method.MethodID, method.Name, "");
					}
				}

				return result;
			}
		}

		new OperationalActionMethodDescriptor Parent
		{
			get { return (OperationalActionMethodDescriptor)base.Parent; }
		}
	}
}
