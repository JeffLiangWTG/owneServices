using Newtonsoft.Json;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class IssueAssignment
	{
		public IssueAssignment(string product, string productArea, string module, bool isRetrievedFromApi = false)
		{
			Product = product ?? string.Empty;
			ProductArea = productArea ?? string.Empty;
			Module = module ?? string.Empty;
			IsRetrievedFromAPI = isRetrievedFromApi;
		}

		public string Product { get; }
		public string ProductArea { get; }
		public string Module { get; }

		[JsonIgnore]
		public bool IsRetrievedFromAPI { get; set; }

		public override bool Equals(object obj)
		{
			return obj is IssueAssignment assignment
				&& assignment.Product == Product
				&& assignment.ProductArea == ProductArea
				&& assignment.Module == Module;
		}

		public override int GetHashCode()
		{
			return Product.GetHashCode() ^ ProductArea.GetHashCode() ^ Module.GetHashCode();
		}

		public override string ToString()
		{
			return Product + "/" + ProductArea + "/" + Module;
		}
	}
}
