using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ExceptionEventsCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return List;
		}

		#region Implementation

		CodeDescriptionPairList List
		{
			get
			{
				if (fList == null)
				{
					fList = new CodeDescriptionPairList();
					foreach (var eventType in new BusinessObjectFactory().Load<ProcessWorkflowExceptionType>(new ZQuery(ProcessWorkflowExceptionTypeSchema.WET_IsActive, true)))
					{
						fList.Add(new CodeDescriptionPair(eventType.WET_Code.ToString(), eventType.WET_DescriptionMultilingual));
					}
				}
				return fList;
			}
		}
		CodeDescriptionPairList fList;

		#endregion
	}
}
