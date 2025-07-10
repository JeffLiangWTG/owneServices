using System.Text;

namespace Enterprise.Customs.KR.Business
{
	public static class StringUtils
	{
		public static string GetSubstringBytes(string value, int outputBytes)
		{
			int inputBytes = Encoding.Unicode.GetByteCount(value);
			if (inputBytes > outputBytes)
			{
				int byteLength = 0;
				int position = 0;
				for (int i = 0; i < value.Length; i++)
				{
					position = i;

					if (value[i] > 0x7f)
					{
						byteLength += 2;
					}
					else
					{
						byteLength += 1;
					}

					if (byteLength > outputBytes)
					{
						position -= 1;
						break;
					}
				}

				value = value.Substring(0, position + 1);
			}
			return value;
		}

		public static string ConvertToXPath(string xmlPath)
		{
			var pathsSplit = xmlPath.Split('/');
			var result = new StringBuilder("/");

			foreach (var partialPath in pathsSplit)
			{
				result.Append($"/*[local-name()='{partialPath}']");
			}

			return result.ToString();
		}
	}
}
