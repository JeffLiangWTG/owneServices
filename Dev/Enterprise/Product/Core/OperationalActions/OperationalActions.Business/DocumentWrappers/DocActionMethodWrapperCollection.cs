using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers
{
	public sealed class DocActionMethodWrapperCollection : DocumentWrapperCollection<DocActionMethodWrapper>
	{
		public static DocActionMethodWrapperCollection New(OperationalActionSupporter supporter)
		{
			DocActionMethodWrapperCollection result = new DocActionMethodWrapperCollection();

			foreach (ActionMethodProviderID id in supporter.Methods.GetAllIds())
			{
				foreach (OperationalActionMethod method in supporter.Methods.GetMethods(id))
				{
					result.Add(DocActionMethodWrapper.New(id.Name, method));
				}
			}

			return result;
		}

		DocActionMethodWrapperCollection()
			: base(null) { }
	}
}
