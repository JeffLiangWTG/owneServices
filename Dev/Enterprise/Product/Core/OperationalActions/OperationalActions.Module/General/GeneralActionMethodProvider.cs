using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Module
{
	public sealed class GeneralActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			var result = new List<OperationalActionMethod>();

			if (typeof(IStmNoteParent).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new ShowEditNoteActionMethod());
			}

			result.Add(new OpenURLActionMethod());
			result.Add(new RunProgramActionMethod());

			return result.ToArray();
		}
	}
}
