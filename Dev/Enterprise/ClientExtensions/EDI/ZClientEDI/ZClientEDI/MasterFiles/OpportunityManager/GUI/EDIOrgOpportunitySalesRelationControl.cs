using System.Collections.Generic;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EDIOrgOpportunitySalesRelationControl : SalesRelationControl
	{
		public EDIOrgOpportunitySalesRelationControl()
		{
			InitializeComponent();
		}

		#region RelatableTypeButtonCaptionPairs

		protected override IEnumerable<KeyValuePair<string, ResourceStringData>> RelatableTypeButtonCaptionPairs
		{
			get
			{
				var relatableTypeButtonCaptionPairs = base.RelatableTypeButtonCaptionPairs.Append(
					new KeyValuePair<string, ResourceStringData>(EDIRelatableActivityTypeList.Codes.Incident,
						Res.GetData("EDIOrgOpportunitySalesRelationControl|Incident", "Incident")));

				return relatableTypeButtonCaptionPairs;
			}
		}

		#endregion
	}
}
