using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class PivotSynchroniser : DestinationDeletableBusinessObjectSynchroniser
	{
		public PivotSynchroniser(CMRHouseBillSynchroniser houseSynchroniser, CusSCAPivot destination, PackLine source, CommonShipment shipment)
			: base(destination, source)
		{
			this.Shipment = shipment;
			this.houseSynchroniser = houseSynchroniser;
		}
		readonly CMRHouseBillSynchroniser houseSynchroniser;

		public new CusSCAPivot Destination
		{
			get { return (CusSCAPivot)base.Destination; }
			protected set { base.Destination = value; }
		}

		public new PackLine Source
		{
			get { return (PackLine)base.Source; }
		}

		public CommonShipment Shipment { get; private set; }

		public CusSCAHouse HouseBill
		{
			get { return fHouseBill; }
			set { fHouseBill = value; }
		}
		CusSCAHouse fHouseBill;

		public void AddPackLineWatch(PackLine packLine, bool synchronisePackline = true)
		{
			if (!AdditionalPackLinesToWatch.Contains(packLine) && packLine != Source)
			{
				AdditionalPackLinesToWatch.Add(packLine);
				HookAdditionalPackLine(packLine);
				if (synchronisePackline)
				{
					SynchronisePackLineValuesForAdditionalPackLineChanges();
				}
			}
		}

		public void RemovePackLineWatch(PackLine packLine, bool synchronisePackline = true)
		{
			if (AdditionalPackLinesToWatch.Remove(packLine))
			{
				UnHookAdditionalPackLine(packLine);
				if (synchronisePackline)
				{
					SynchronisePackLineValuesForAdditionalPackLineChanges();
				}
			}
		}

		public bool IsWatchingPackLine(PackLine packLine) => fAdditionalPackLinesToWatch?.Contains(packLine) ?? false;

		#region Implementation

		protected internal List<PackLine> AdditionalPackLinesToWatch => fAdditionalPackLinesToWatch ?? (fAdditionalPackLinesToWatch = new List<PackLine>());
		List<PackLine> fAdditionalPackLinesToWatch;

		protected ForwardingConsol ParentConsol => HouseBill?.OceanBill?.Consol ?? Destination.OceanBill?.Consol;

		protected override void HookSynchronisers()
		{
			var packTypeSynchroniser = new FieldSynchroniser(Destination.CV_PackageTypeInfo, Source.JL_F3_NKPackTypeInfo, true);
			packTypeSynchroniser.Format += PackTypeSynchroniser_Format;
			Synchronisers.Add(packTypeSynchroniser);

			var packageCountSynchroniser = new FieldSynchroniser(Destination.CV_PackageCountInfo, Source.JL_PackageCountInfo, true);
			packageCountSynchroniser.Format += PackageCountSynchroniser_Format;
			Synchronisers.Add(packageCountSynchroniser);

			var weightSynchroniser = new FieldSynchroniser(Destination.CV_WeightInfo, Source.JL_ActualWeightInfo, true);
			weightSynchroniser.Format += WeightSynchroniser_Format;
			Synchronisers.Add(weightSynchroniser);

			var weightUQSynchroniser = new FieldSynchroniser(Destination.CV_WeightInfo, Source.JL_ActualWeightUQInfo, true);
			weightUQSynchroniser.Format += WeightSynchroniser_Format;
			Synchronisers.Add(weightUQSynchroniser);

			var volumeSynchroniser = new FieldSynchroniser(Destination.CV_VolumeInfo, Source.JL_ActualVolumeInfo, true);
			volumeSynchroniser.Format += VolumeSynchroniser_Format;
			Synchronisers.Add(volumeSynchroniser);

			var volumeUQSynchroniser = new FieldSynchroniser(Destination.CV_VolumeInfo, Source.JL_ActualVolumeUQInfo, true);
			volumeUQSynchroniser.Format += VolumeSynchroniser_Format;
			Synchronisers.Add(volumeUQSynchroniser);

			var marksAndNumbersSynchroniser = new FieldSynchroniser(Destination.CV_MarksAndNumbersInfo, Source.JL_MarksAndNumbersInfo, true);
			marksAndNumbersSynchroniser.Format += MarksAndNumbersSynchroniser_Format;
			Synchronisers.Add(marksAndNumbersSynchroniser);

			if (Shipment != null)
			{
				var shipmentGoodsDescriptionSynchroniser = new FieldSynchroniser(Destination.CV_GoodsDescriptionInfo, Shipment.JS_GoodsDescriptionInfo, true);
				shipmentGoodsDescriptionSynchroniser.Format += GoodsDescriptionSynchroniser_Format;
				Synchronisers.Add(shipmentGoodsDescriptionSynchroniser);

				var shipmentMarksAndNumbersSynchroniser = new FieldSynchroniser(Destination.CV_MarksAndNumbersInfo, Shipment.JS_MarksAndNumbersInfo, true);
				shipmentMarksAndNumbersSynchroniser.Format += ShipmentMarksAndNumbersSynchroniser_Format;
				Synchronisers.Add(shipmentMarksAndNumbersSynchroniser);

				Shipment.Notes.VisibleNotes.CountChanged += VisibleNotes_CountChanged;
				foreach (StmNote note in Source.Notes.ClientVisibleNotes)
				{
					note.ST_DescriptionInfo.ValueChanged += ST_DescriptionInfo_ValueChanged;
					if (note.ST_Description == PredefinedNoteTypes.Instance.MarksAndNumbers.Description)
					{
						note.ST_NoteTextInfo.ValueChanged += ST_NoteTextInfo_ValueChanged;
					}
				}
			}

			HookCollectionSynchronisers();
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			UnHookCollectionSynchronisers();
		}

		protected void HookAdditionalPackLine(PackLine packLine)
		{
			packLine.JL_F3_NKPackTypeInfo.ValueChanged += AdditionalPackLineJL_F3_NKPackTypeInfo_ValueChanged;
			packLine.JL_PackageCountInfo.ValueChanged += AdditionalPackLineJL_PackageCountInfo_ValueChanged;
			packLine.JL_ActualVolumeInfo.ValueChanged += AdditionalPackLineJL_ActualVolumeInfo_ValueChanged;
			packLine.JL_ActualVolumeUQInfo.ValueChanged += AdditionalPackLineJL_ActualVolumeInfo_ValueChanged;
			packLine.JL_ActualWeightInfo.ValueChanged += AdditionalPackLineJL_ActualWeightInfo_ValueChanged;
			packLine.JL_ActualWeightUQInfo.ValueChanged += AdditionalPackLineJL_ActualWeightInfo_ValueChanged;
			packLine.JL_MarksAndNumbersInfo.ValueChanged += AdditionalPacklineJL_MarksAndNumbersInfo_ValueChanged;

			packLine.Containers.CountChanged += AdditionalPackLineContainers_CountChanged;
		}

		protected void UnHookAdditionalPackLine(PackLine packLine)
		{
			packLine.JL_F3_NKPackTypeInfo.ValueChanged -= AdditionalPackLineJL_F3_NKPackTypeInfo_ValueChanged;
			packLine.JL_PackageCountInfo.ValueChanged -= AdditionalPackLineJL_PackageCountInfo_ValueChanged;
			packLine.JL_ActualVolumeInfo.ValueChanged -= AdditionalPackLineJL_ActualVolumeInfo_ValueChanged;
			packLine.JL_ActualVolumeUQInfo.ValueChanged -= AdditionalPackLineJL_ActualVolumeInfo_ValueChanged;
			packLine.JL_ActualWeightInfo.ValueChanged -= AdditionalPackLineJL_ActualWeightInfo_ValueChanged;
			packLine.JL_ActualWeightUQInfo.ValueChanged -= AdditionalPackLineJL_ActualWeightInfo_ValueChanged;
			packLine.JL_MarksAndNumbersInfo.ValueChanged -= AdditionalPacklineJL_MarksAndNumbersInfo_ValueChanged;

			packLine.Containers.CountChanged -= AdditionalPackLineContainers_CountChanged;
		}

		protected void SynchronisePackLineValuesForAdditionalPackLineChanges()
		{
			if (IsEnabled && !Source.IsDeleted && !Destination.IsDeleted)
			{
				AdditionalPackLineJL_F3_NKPackTypeInfo_ValueChanged(this, EventArgs.Empty);
				AdditionalPackLineJL_PackageCountInfo_ValueChanged(this, EventArgs.Empty);
				AdditionalPackLineJL_ActualVolumeInfo_ValueChanged(this, EventArgs.Empty);
				AdditionalPackLineJL_ActualWeightInfo_ValueChanged(this, EventArgs.Empty);
				AdditionalPacklineJL_MarksAndNumbersInfo_ValueChanged(this, EventArgs.Empty);
			}
		}

		void HookCollectionSynchronisers()
		{
			Source.Containers.CountChanged += PackLineContainers_CountChanged;
			HookNotesSynchroniser();
		}

		void UnHookCollectionSynchronisers()
		{
			Source.Containers.CountChanged -= PackLineContainers_CountChanged;
			if (Shipment != null)
			{
				Shipment.Notes.VisibleNotes.CountChanged -= VisibleNotes_CountChanged;
			}

			if (fAdditionalPackLinesToWatch != null)
			{
				var packlinesCached = AdditionalPackLinesToWatch.ToArray();
				foreach (var packline in packlinesCached)
				{
					RemovePackLineWatch(packline);
				}
			}

			UnHookNotesSynchroniser();
		}

		protected void HookNotesSynchroniser()
		{
			if (Shipment != null)
			{
				var marksAndNubmersNotes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				foreach (var marksAndNumbersNote in marksAndNubmersNotes)
				{
					marksAndNumbersNote.ST_NoteTextInfo.ValueChanged += MarksAndNumbersNoteText_ValueChanged;
				}

				var goodsDescriptionNotes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
				foreach (var goodsDescriptionNote in goodsDescriptionNotes)
				{
					goodsDescriptionNote.ST_NoteTextInfo.ValueChanged += GoodsDescriptionNoteText_ValueChanged;
				}

				Source.Notes.NoteAdded += Notes_NoteAdded;
			}
		}

		protected void UnHookNotesSynchroniser()
		{
			if (Shipment != null && !Shipment.IsDeleted)
			{
				Source.Notes.NoteAdded -= Notes_NoteAdded;

				var marksAndNubmersNotes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				foreach (var marksAndNumbersNote in marksAndNubmersNotes)
				{
					marksAndNumbersNote.ST_NoteTextInfo.ValueChanged -= MarksAndNumbersNoteText_ValueChanged;
				}

				var goodsDescriptionNotes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
				foreach (var goodsDescriptionNote in goodsDescriptionNotes)
				{
					goodsDescriptionNote.ST_NoteTextInfo.ValueChanged -= GoodsDescriptionNoteText_ValueChanged;
				}
			}
		}

		protected abstract void PackTypeSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e);

		protected abstract void PackageCountSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e);

		void WeightSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			ZString unitOfQuantity = Source.JL_ActualWeightUQ;
			ZDecimal weight = GetPackLineWeight(Source, unitOfQuantity);
			foreach (PackLine additionalPackLine in AdditionalPackLinesToWatch)
			{
				weight += GetPackLineWeight(additionalPackLine, unitOfQuantity);
			}

			if (unitOfQuantity == Core.Constants.Weight.Kilograms && !weight.IsWithinSqlPrecisionAndScale(CusSCAPivotSchema.CV_Weight.Precision, CusSCAPivotSchema.CV_Weight.Scale))
			{
				weight = new ZDecimal(Core.Constants.Weight.Convert(weight, Core.Constants.Weight.Kilograms, Core.Constants.Weight.Tonnes));
				unitOfQuantity = Core.Constants.Weight.Tonnes;
			}

			if (unitOfQuantity == Core.Constants.Weight.Tonnes)
			{
				unitOfQuantity = Core.Constants.Weight.ShortTons;  // This is done here as weight is NOT to be recalculated when replacing Shipping PackingLine Tonnes code ("T") with Customs SCA Packing Tonnes code ("TN")
				if (!weight.IsWithinSqlPrecisionAndScale(CusSCAPivotSchema.CV_Weight.Precision, CusSCAPivotSchema.CV_Weight.Scale))
				{
					weight = new ZDecimal(Core.Constants.Weight.Convert(weight, Core.Constants.Weight.Tonnes, Core.Constants.Weight.Kilotonnes));
					unitOfQuantity = Core.Constants.Weight.Kilotonnes;
				}
			}

			if (Destination.CV_WeightUQ != unitOfQuantity)
			{
				Destination.CV_WeightUQ = unitOfQuantity;
			}

			Destination.CV_NetWeight = weight;
			e.Value = weight;
		}

		ZDecimal GetPackLineWeight(PackLine packLine, ZString unitOfQuantity)
		{
			ZDecimal result = packLine.JL_ActualWeight;
			if (!unitOfQuantity.IsEmpty && !packLine.JL_ActualWeightUQ.IsEmpty && packLine.JL_ActualWeightUQ != unitOfQuantity)
			{
				try
				{
					result = new ZDecimal(Enterprise.Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, unitOfQuantity));
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					result = packLine.JL_ActualWeight;
				}
			}

			return result;
		}

		void VolumeSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			ZString unitOfQuantity = Enterprise.Core.Constants.Volume.CubicMetres;
			ZDecimal volume = GetPackLineVolume(Source, unitOfQuantity);
			foreach (PackLine additionalPackLine in AdditionalPackLinesToWatch)
			{
				volume += GetPackLineVolume(additionalPackLine, unitOfQuantity);
			}

			e.Value = volume;
		}

		ZDecimal GetPackLineVolume(PackLine packLine, ZString unitOfQuantity)
		{
			ZDecimal result = packLine.JL_ActualVolume;
			if (!unitOfQuantity.IsEmpty && !packLine.JL_ActualVolumeUQ.IsEmpty && packLine.JL_ActualVolumeUQ != unitOfQuantity)
			{
				try
				{
					result = new ZDecimal(Enterprise.Core.Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ, unitOfQuantity));
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					result = packLine.JL_ActualVolume;
				}
			}

			return result;
		}

		void PackLineContainers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			try
			{
				if (!IsSenderRefreshingByDataRefreshBus(sender) && IsEnabled && !Destination.IsDeleted
					&& e.BizObject is CommonContainer packLineContainer && !packLineContainer.IsDeleted)
				{
					if (e.ItemAdded)
					{
						var oceanBillContainers = Destination.OceanBill?.Containers;
						if (oceanBillContainers != null)
						{
							var cusContainer = oceanBillContainers.Find(packLineContainer.JC_ContainerNum);
							if (cusContainer == null)
							{
								cusContainer = Destination.OceanBill.Containers.AddNew();
								cusContainer.CN_ContainerNumber = packLineContainer.JC_ContainerNum;
							}

							if (Destination.CV_CN.IsEmpty && !HasPivotOnContainer(cusContainer))
							{
								Destination.CV_CN = cusContainer.PK;
								ReallocateAdditionalPackLinesToWatch();
								Destination.CV_MarksAndNumbers = CalculateMarksAndNumbers();
							}

							if (Destination.CV_CN != cusContainer.PK)
							{
								ReallocateSourcePackline();
							}
						}
					}
					else
					{
						ReallocateSourcePackline();
					}
				}
			}
			catch (System.Data.RowNotInTableException)
			{
				SetEnabled(false, DetectEnabled);
			}
		}

		bool HasPivotOnContainer(CusSCAContainer cusContainer)
		{
			return Destination.HouseBill.Pivot.Find(x => x.CV_CN == cusContainer.PK).Any();
		}

		public void ReallocateSourcePackline()
		{
			var watchedPacklinesCached = AdditionalPackLinesToWatch.ToArray();

			Destination.HouseBill.Pivot.RemoveAndDelete(Destination);
			Destination = null;

			houseSynchroniser?.AddPackline(Source);
			foreach (var watchedLine in watchedPacklinesCached)
			{
				houseSynchroniser?.AddPackline(watchedLine);
			}
		}

		void ReallocateAdditionalPackLinesToWatch()
		{
			foreach (var watchedLine in AdditionalPackLinesToWatch.ToArray())
			{
				AdditionalPackLinesToWatch.Remove(watchedLine);
				houseSynchroniser?.AddPackline(watchedLine);
			}
		}

		void AdditionalPackLineContainers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsSenderRefreshingByDataRefreshBus(sender) && IsEnabled && !Destination.IsDeleted
				&& sender is CommonContainerManyToManyCollection packlineContainers)
			{
				var packline = packlineContainers.ParentPackLine;
				RemovePackLineWatch(packline);
				if (packline.Shipment != null) // not deleting
				{
					houseSynchroniser?.AddPackline(packline);
				}
			}
		}

		void VisibleNotes_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is StmNote note && !note.IsDeleted)
			{
				if (e.ItemAdded)
				{
					note.ST_DescriptionInfo.ValueChanged += ST_DescriptionInfo_ValueChanged;
					if (note.ST_Description == PredefinedNoteTypes.Instance.MarksAndNumbers.Description)
					{
						note.ST_NoteTextInfo.ValueChanged += ST_NoteTextInfo_ValueChanged;
						Destination.CV_MarksAndNumbers = CalculateMarksAndNumbers();
					}
				}
				else if (e.ItemRemoved)
				{
					note.ST_DescriptionInfo.ValueChanged -= ST_DescriptionInfo_ValueChanged;
					note.ST_NoteTextInfo.ValueChanged -= ST_NoteTextInfo_ValueChanged;
					if (note.ST_Description == PredefinedNoteTypes.Instance.MarksAndNumbers.Description)
					{
						Destination.CV_MarksAndNumbers = CalculateMarksAndNumbers();
					}
				}
			}
		}

		void ST_NoteTextInfo_ValueChanged(object sender, EventArgs e)
		{
			Destination.CV_MarksAndNumbers = CalculateMarksAndNumbers();
		}

		void ST_DescriptionInfo_ValueChanged(object sender, EventArgs e)
		{
			if (sender is StmNote note && !note.IsDeleted)
			{
				if (note.ST_Description == PredefinedNoteTypes.Instance.MarksAndNumbers.Description)
				{
					note.ST_NoteTextInfo.ValueChanged += ST_NoteTextInfo_ValueChanged;
				}
				else
				{
					note.ST_NoteTextInfo.ValueChanged -= ST_NoteTextInfo_ValueChanged;
					Destination.CV_MarksAndNumbers = CalculateMarksAndNumbers();
				}
			}
		}

		#endregion

		void AdditionalPackLineJL_F3_NKPackTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var args = new FieldSynchroniser.ConvertEventArgs(Source.JL_F3_NKPackType, typeof(ZString));
			PackTypeSynchroniser_Format(sender, args);
			Destination.CV_PackageType = (ZString)args.Value;
		}

		void AdditionalPackLineJL_PackageCountInfo_ValueChanged(object sender, EventArgs e)
		{
			var args = new FieldSynchroniser.ConvertEventArgs(Source.JL_PackageCount, typeof(ZInt));
			PackageCountSynchroniser_Format(sender, args);
			Destination.CV_PackageCount = (ZInt)args.Value;
		}

		void AdditionalPackLineJL_ActualVolumeInfo_ValueChanged(object sender, EventArgs e)
		{
			var args = new FieldSynchroniser.ConvertEventArgs(Source.JL_ActualVolume, typeof(ZDecimal));
			VolumeSynchroniser_Format(sender, args);
			Destination.CV_Volume = (ZDecimal)args.Value;
		}

		void AdditionalPackLineJL_ActualWeightInfo_ValueChanged(object sender, EventArgs e)
		{
			var args = new FieldSynchroniser.ConvertEventArgs(Source.JL_ActualWeight, typeof(ZDecimal));
			WeightSynchroniser_Format(sender, args);
			Destination.CV_Weight = (ZDecimal)args.Value;
		}

		void AdditionalPacklineJL_MarksAndNumbersInfo_ValueChanged(object sender, EventArgs e)
		{
			Destination.CV_MarksAndNumbers = CalculateMarksAndNumbers();
		}

		void GoodsDescriptionSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			ZString result = ZString.Empty;
			if (e.Value is ZString)
			{
				result = (ZString)e.Value;
				if (Shipment != null && !Shipment.IsDeleted)
				{
					result = Shipment.DetailedGoodsDescriptionNoteText.IsEmpty ?
						Shipment.JS_GoodsDescription : Shipment.DetailedGoodsDescriptionNoteText;
				}
			}
			e.Value = result.SubstringSafe(0, CusSCAPivotSchema.CV_GoodsDescription.MaxLength);
		}

		void MarksAndNumbersSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			e.Value = CalculateMarksAndNumbers();
		}

		void ShipmentMarksAndNumbersSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			e.Value = CalculateMarksAndNumbers();
		}

		void Notes_NoteAdded(NoteAddedEventArgs args)
		{
			if (args.NoteAdded.ST_NoteType == PredefinedNoteTypes.Instance.MarksAndNumbers.Description)
			{
				args.NoteAdded.ST_NoteTextInfo.ValueChanged += MarksAndNumbersNoteText_ValueChanged;
			}
			else if (args.NoteAdded.ST_NoteType == PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description)
			{
				args.NoteAdded.ST_NoteTextInfo.ValueChanged += GoodsDescriptionNoteText_ValueChanged;
			}
		}

		void MarksAndNumbersNoteText_ValueChanged(object sender, EventArgs e)
		{
			if (sender is StmNote note && !note.IsDeleted)
			{
				Destination.CV_MarksAndNumbers = CalculateMarksAndNumbers();
			}
		}

		void GoodsDescriptionNoteText_ValueChanged(object sender, EventArgs e)
		{
			StmNote note = sender as StmNote;
			if (note != null && !note.IsDeleted)
			{
				ZString shortGoodsDescription = note.ST_NoteText;
				if (Source != null && !Source.IsDeleted && Shipment != null && !Shipment.IsDeleted)
				{
					shortGoodsDescription = Shipment.JS_GoodsDescription;
				}
				var args = new FieldSynchroniser.ConvertEventArgs(shortGoodsDescription, typeof(ZString));
				GoodsDescriptionSynchroniser_Format(this, args);
				Destination.CV_GoodsDescription = (ZString)args.Value;
			}
		}

		ZString CalculateMarksAndNumbers()
		{
			ZString result;

			if (Shipment != null && !Shipment.IsDeleted)
			{
				result = CalculateMarksAndNumbersFromPackingLines();
				if (result.IsEmpty)
				{
					result = Shipment.JS_MarksAndNumbers;
					if (result.IsEmpty)
					{
						result = CalculateMarksAndNumbersFromContainer();
					}
				}
			}
			else
			{
				result = Destination.CV_MarksAndNumbers;
			}

			return result.SubstringSafe(0, CusSCAPivot.Schema.CV_MarksAndNumbersMaxLength).ToUpper();
		}

		ZString CalculateMarksAndNumbersFromPackingLines()
		{
			var allPackLines = AdditionalPackLinesToWatch.Append(Source);
			var marksAndNumbers = allPackLines.Where(x => !x.JL_MarksAndNumbers.IsEmpty)
											  .Select(x => x.JL_MarksAndNumbers)
											  .OrderBy(x => x);
			return string.Join(",", marksAndNumbers);
		}

		ZString CalculateMarksAndNumbersFromContainer()
		{
			var result = ZString.Empty;

			var container = Destination.Container;
			if (container != null)
			{
				if (Destination.ShouldDefaultContainerNoAndHouseBillToMarksAndNumbers)
				{
					result = container.CN_ContainerNumber;
					result += !result.IsEmpty ? "/" : string.Empty;
					result += Destination.HouseBill.CA_HouseBill;
				}
				else if (Shipment.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.FCL)
				{
					result = container.CN_ContainerNumber;
				}
			}

			return result;
		}
	}
}
