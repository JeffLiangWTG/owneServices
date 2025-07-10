using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSlideshowPivot : AutoBMBoardSlideshowPivot
	{
		public BMBoardSlideshowPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var proposedValue = BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.Value * 60;

			MC_DurationInSeconds = (ZShort)Math.Min(short.MaxValue, proposedValue);
		}

		#endregion

		#region Loading

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (Board != null)
			{
				systemPK = Board.MB_FS_System;
			}
		}

		#endregion

		#region Properties

		[List("Lookups.Systems")]
		public ZGuid SystemPK
		{
			get { return systemPK; }
			set { SetNonPersistentPropertyValue(SystemPKInfo, ref systemPK, value); }
		}

		ZGuid systemPK;

		public ZPropertyInfo SystemPKInfo
		{
			get { return GetZPropertyInfo(nameof(SystemPK)); }
		}

		public BMSystem System
		{
			get { return Factory.Load<BMSystem>(SystemPK); }
		}

		[RelatedBusinessObject("Board")]
		[List("Lookups.Boards")]
		public override ZGuid MC_MB_Board
		{
			get { return base.MC_MB_Board; }
			set { base.MC_MB_Board = value; }
		}

		public BMBoard Board
		{
			get { return Factory.Load<BMBoard>(MC_MB_Board); }
		}

		[RelatedBusinessObject("Slideshow")]
		public override ZGuid MC_MD_Slideshow
		{
			get { return base.MC_MD_Slideshow; }
			set { base.MC_MD_Slideshow = value; }
		}

		public BMBoardSlideshow Slideshow
		{
			get { return Factory.Load<BMBoardSlideshow>(MC_MD_Slideshow); }
		}

		#endregion
	}
}
