function CalculateVolume(volumeControlID, piecesControlID, lengthControlID, widthControlID, heightControlID, dimUnitControlID, volumeUnitControlID) {
    if ($(volumeControlID) && $(piecesControlID) && $(lengthControlID) && $(widthControlID) && $(heightControlID) && $(dimUnitControlID) && $(volumeUnitControlID)) {
        ExecuteServiceMethod("CalculateVolume", 
                             JSON.encode({ VolumeControlID : volumeControlID,
							 DefaultVolume: $(volumeControlID).get('value'),
							 Pieces: $(piecesControlID).get('value'),
							 Length: $(lengthControlID).get('value'),
							 Width: $(widthControlID).get('value'),
							 Height: $(heightControlID).get('value'),
							 DimUnit: $(dimUnitControlID).get('value'),
							 VolumeUnit: $(volumeUnitControlID).get('value')
				             }));
    }
}
