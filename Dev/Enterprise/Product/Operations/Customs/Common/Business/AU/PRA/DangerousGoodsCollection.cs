using System.Collections;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Customs.Common.AU
{
	public class DangerousGoodsCollection : IEnumerable
	{
		public DangerousGoodsCollection()
		{
			fCollection = new ArrayList();
		}

		#region Standard Typed Collection Code

		public ContainerMessagingDangerousGoods this[int index]
		{
			get { return (ContainerMessagingDangerousGoods)fCollection[index]; }
		}

		public ContainerMessagingDangerousGoods AddNew()
		{
			ContainerMessagingDangerousGoods newObject = new ContainerMessagingDangerousGoods();
			fCollection.Add(newObject);
			return newObject;
		}

		public int Count
		{
			get { return fCollection.Count; }
		}

		protected ArrayList fCollection;

		public IEnumerator GetEnumerator()
		{
			return fCollection.GetEnumerator();
		}

		#endregion

		public string GetErrors()
		{
			StringBuilder result = new StringBuilder();
			foreach (ContainerMessagingDangerousGoods goods in this)
			{
				ZString errors = goods.GetErrors();
				if (!errors.IsEmpty)
				{
					if (goods.ShipmentReference.IsEmpty)
					{
						result.Append(Res.GetString("86951723-7B37-47EE-B83C-0723BFDC7658", "Dangerous Goods Info Missing:\r\n{0}", errors));
					}
					else
					{
						result.Append(Res.GetString("029A42BC-DC90-4713-BC6E-971F9543A9E3", "Shipment Ref: {0} has Dangerous Goods Info Missing:\r\n{1}", goods.ShipmentReference.ToString(), errors));
					}
				}
			}
			return result.ToString();
		}
	}
}
