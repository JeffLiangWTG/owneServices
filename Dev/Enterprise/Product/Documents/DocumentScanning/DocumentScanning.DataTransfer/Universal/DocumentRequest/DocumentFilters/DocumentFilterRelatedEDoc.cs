using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	class DocumentFilterRelatedEDoc : DocumentFilterBase
	{
		public DocumentFilterRelatedEDoc()
		{
			Values = new List<ZString>();
		}
		internal List<ZString> Values;

		public override void AddFilterValue(ZString value)
		{
			var upperCaseValue = value.Trim().ToUpper();
			if (!Values.Contains(upperCaseValue) && !upperCaseValue.IsEmpty)
			{
				Values.Add(upperCaseValue);
			}
		}

		public override bool IsMatch(IeDoc eDoc)
		{
			var result = Values.Count == 0;
			if (!result && eDoc.ParentMain is IStorageMain storageMain)
			{
				var documentOwnerDescription = storageMain.DocumentOwnerDescription.ToUpper();

				Values.ForEach(value =>
				{
					var valueTrimEndAsterisks = value.TrimEnd('*');
					result |= valueTrimEndAsterisks.Length == value.Length ? documentOwnerDescription.Equals(value) : documentOwnerDescription.StartsWith(valueTrimEndAsterisks);
				});
			}
			return result;
		}
	}
}
