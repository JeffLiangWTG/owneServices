using System.Linq;
using System.Web.Script.Serialization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class UpdateListResponseToken : WebServiceResponseActionToken
	{
		#region Constructors

		public UpdateListResponseToken(string controlID, CodeDescriptionPairList values)
			: base(WebServiceResponseActions.UpdateList, controlID, ToJSON(values))
		{
		}

		#endregion

		static string ToJSON(CodeDescriptionPairList values) => new JavaScriptSerializer().Serialize(values.Cast<CodeDescriptionPair>().Select(p => new { Code = p.Code, Description = p.Description }));
	}
}
