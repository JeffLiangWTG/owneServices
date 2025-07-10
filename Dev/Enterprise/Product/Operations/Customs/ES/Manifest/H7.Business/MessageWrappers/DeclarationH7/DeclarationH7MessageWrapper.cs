using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class DeclarationH7MessageWrapper(AsycudaBill bill, ICertificateProvider certificate)
	: H7CommonSendMessageWrapper(bill, certificate), IDeclarationH7MessageDataProvider
{
	public IDeclarationH7Header Header => new DeclarationH7HeaderWrapper(Bill);

	public IReadOnlyCollection<IDeclarationH7Line> Lines
	{
		get
		{
			if (declarationH7Lines == null)
			{
				var declarationH7LinesList = new List<IDeclarationH7Line>();

				if (Bill?.PackedItems != null && Bill.PackedItems.Count != 0)
				{
					var countedPackages = new List<ZGuid>();
					foreach (var item in Bill.PackedItems)
					{
						var packagePKs = item.PackagesPivot.Where(p => !countedPackages.Contains(p.APP_APA_Pack))
							.Select(p => p.APP_APA_Pack).Distinct().ToList();
						declarationH7LinesList.Add(new DeclarationH7LineWrapper(item, packagePKs.Count));
						countedPackages.AddRange(packagePKs);
					}
				}
				declarationH7Lines = declarationH7LinesList.AsReadOnly();
			}

			return declarationH7Lines;
		}
	}
	IReadOnlyCollection<IDeclarationH7Line> declarationH7Lines;
}
