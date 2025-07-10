using System.IO;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class TestFileHelper
	{
		public static ZString SerializeResponse(string filePath)
		{
			var response = ZString.Empty;
			using (var reader = File.OpenText(filePath))
			{
				var textString = reader.ReadToEnd();
				response = textString;
			}
			return response;
		}
	}
}
