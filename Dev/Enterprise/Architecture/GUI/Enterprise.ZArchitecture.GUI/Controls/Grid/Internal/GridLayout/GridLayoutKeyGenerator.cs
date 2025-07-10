using System;
using System.Security.Cryptography;
using System.Text;
using Enterprise.ZArchitecture;

namespace Enterprise.Core.Forms.Internal
{
	internal sealed class GridLayoutKeyGenerator
	{
		public GridLayoutKeyGenerator(ZGrid grid, object dataSource, string dataMember, bool useFullGridParentsPath)
		{
			Grid = grid;
			DataSource = dataSource.GetType().FullName;
			DataMember = dataMember;
			this.useFullGridParentsPath = useFullGridParentsPath;
		}

		public string GetContextKey()
		{
			var result = new StringBuilder();
			result.Append(BindingKey);
			result.Append(Grid.GetParentControlNames(useFullGridParentsPath));

			return "GridLayout" + GetMD5Hash(result.ToString());
		}

		#region Implementation

		readonly ZGrid Grid;
		readonly string DataSource;
		readonly string DataMember;
		readonly bool useFullGridParentsPath;

		StringBuilder BindingKey
		{
			get
			{
				var result = new StringBuilder();
				result.Append(Grid.LayoutKey);
				result.Append("|");
				result.Append(DataSource);
				result.Append("|");
				result.Append(DataMember);
				result.Append("|");
				result.Append(Grid.GetType());

				return result;
			}
		}

		string GetMD5Hash(string value)
		{
			var md5Bytes = MD5.Create().ComputeHash(new ASCIIEncoding().GetBytes(value));
			return Convert.ToBase64String(md5Bytes);
		}

		#endregion
	}
}
