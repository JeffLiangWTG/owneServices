using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class GLRollupLevelCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return AddGLRollupLevels();
		}

		#region Implementation

#if DEBUG
		public
#endif
		CodeDescriptionPairList AddGLRollupLevels()
		{
			CodeDescriptionPairList fList = new CodeDescriptionPairList();

			fList.Add(new CodeDescriptionPair("0", Res.GetString("207ac78b-ed8c-4010-9922-93a1584f28a0", "No Roll up")));

			string rollupFormat1 = "";
			string rollupFormat2 = "";
			string rollupFormat3 = "";
			string currentFormat = AccGLHeader.CurrentGLAccountFormat;
			int pos1 = currentFormat.IndexOf(".");
			int pos2 = -1;
			int pos3 = -1;

			if (pos1 > -1)
			{
				pos2 = currentFormat.IndexOf(".", pos1 + 1);

				if (pos2 > -1)
				{
					pos3 = currentFormat.IndexOf(".", pos2 + 1);
				}
			}

			if (pos1 > -1)
			{
				rollupFormat1 = currentFormat.Substring(0, pos1 + 1) + AccountFormatFillDash(currentFormat, pos1 + 1);
				fList.Add(new CodeDescriptionPair("1", rollupFormat1));
			}

			if (pos2 > -1)
			{
				rollupFormat2 = currentFormat.Substring(0, pos2 + 1) + AccountFormatFillDash(currentFormat, pos2 + 1);
				fList.Add(new CodeDescriptionPair("2", rollupFormat2));
			}

			if (pos3 > -1)
			{
				rollupFormat3 = currentFormat.Substring(0, pos3 + 1) + AccountFormatFillDash(currentFormat, pos3 + 1);
				fList.Add(new CodeDescriptionPair("3", rollupFormat3));
			}

			return fList;
		}

		string AccountFormatFillDash(string rollupFormat, int startPosition)
		{
			return rollupFormat.Substring(startPosition).Replace("X", "-");
		}

		#endregion
	}
}
